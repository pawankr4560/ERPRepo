using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Order;

namespace WebApp.Service.Order
{
    public class OrderService : IOrderService
    {
        private readonly IMapper _mapper;
        private readonly WebAppDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrderService(
            IMapper mapper,
            WebAppDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<bool> CreateOrder(List<CreateOrderRequestModel> model)
        {
            var request = _mapper.Map<List<OrderHistory>>(model);
            _dbContext.OrderHistory.AddRange(request);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<dynamic> GetOrders()
        {
            return await _dbContext.OrderHistory.OrderByDescending(x => x.CreatedOn).ToListAsync();
        }

        public async Task<dynamic> GetSalesItemsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status && x.StockQty > 0)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    id = x.Id.ToString(),
                    code = x.Code,
                    name = x.Name,
                    category = x.Categorie,
                    stockQty = x.StockQty,
                    price = x.Price,
                    image = x.Image,
                    description = x.Description
                })
                .ToListAsync();
        }

        public async Task<SalesOrderRazorpayOrderResponse> CreateSalesOrderCheckoutAsync(
            SalesOrderCheckoutRequest request,
            string userId)
        {
            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];

            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
            {
                throw new InvalidOperationException("Razorpay is not configured. Add KeyId and KeySecret in appsettings.");
            }

            ValidateSalesOrderRequest(request);
            var subtotal = await CalculateSubtotalAsync(request.Items);
            var serviceFee = await GetCheckoutServiceFeeAmountAsync(subtotal);
            var total = subtotal + serviceFee;
            var amountPaise = ToPaise(total);
            var receipt = $"sales_{DateTime.UtcNow:yyyyMMddHHmmss}";

            var payload = new
            {
                amount = amountPaise,
                currency = "INR",
                receipt,
                notes = new Dictionary<string, string>
                {
                    ["userId"] = userId,
                    ["orderType"] = NormalizeOrderType(request.OrderType),
                    ["subtotal"] = subtotal.ToString("0.00"),
                    ["serviceFee"] = serviceFee.ToString("0.00")
                }
            };

            var client = CreateRazorpayClient(keyId, keySecret);
            using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("orders", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Unable to create Razorpay sales order: {body}");
            }

            using var document = JsonDocument.Parse(body);
            var orderId = document.RootElement.GetProperty("id").GetString()
                ?? throw new InvalidOperationException("Razorpay order id missing in response.");

            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);

            return new SalesOrderRazorpayOrderResponse
            {
                KeyId = keyId,
                OrderId = orderId,
                Subtotal = subtotal,
                ServiceFeeAmount = serviceFee,
                TotalAmount = total,
                AmountPaise = amountPaise,
                Currency = "INR",
                CustomerName = $"{user?.FirstName} {user?.LastName}".Trim(),
                CustomerEmail = user?.Email ?? string.Empty,
                CustomerPhone = user != null && user.Phone > 0 ? user.Phone.ToString() : string.Empty
            };
        }

        public async Task<SalesOrderVerifyResponse> VerifySalesOrderPaymentAsync(
            SalesOrderVerifyRequest request,
            string userId)
        {
            var keySecret = _configuration["Razorpay:KeySecret"];
            if (string.IsNullOrWhiteSpace(keySecret))
            {
                throw new InvalidOperationException("Razorpay is not configured.");
            }

            ValidateSalesOrderRequest(request);

            if (string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||
                string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
                string.IsNullOrWhiteSpace(request.RazorpaySignature))
            {
                throw new InvalidOperationException("Payment verification details are incomplete.");
            }

            var expectedSignature = ComputeSignature(
                $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}",
                keySecret);

            if (!string.Equals(expectedSignature, request.RazorpaySignature, StringComparison.OrdinalIgnoreCase))
            {
                return new SalesOrderVerifyResponse
                {
                    Success = false,
                    Message = "Payment verification failed. Invalid signature."
                };
            }

            var duplicatePayment = await _dbContext.OrderHistory.AnyAsync(x =>
                x.TransactionReference == request.RazorpayPaymentId);
            if (duplicatePayment)
            {
                return new SalesOrderVerifyResponse
                {
                    Success = true,
                    Message = "This order payment was already recorded."
                };
            }

            var productIds = request.Items.Select(x => Guid.Parse(x.ProductId)).ToList();
            var products = await _dbContext.Products
                .Where(x => productIds.Contains(x.Id) && !x.IsDeleted && x.Status)
                .ToListAsync();

            if (products.Count != request.Items.Count)
            {
                throw new InvalidOperationException("One or more selected items are unavailable.");
            }

            foreach (var item in request.Items)
            {
                var product = products.First(x => x.Id == Guid.Parse(item.ProductId));
                if (product.StockQty < item.Quantity)
                {
                    throw new InvalidOperationException($"{product.Name} has only {product.StockQty} in stock.");
                }
            }

            var subtotal = request.Items.Sum(item =>
            {
                var product = products.First(x => x.Id == Guid.Parse(item.ProductId));
                return Convert.ToDecimal(product.Price) * item.Quantity;
            });
            var serviceFee = await GetCheckoutServiceFeeAmountAsync(subtotal);
            var chargedAmount = subtotal + serviceFee;

            foreach (var item in request.Items)
            {
                var product = products.First(x => x.Id == Guid.Parse(item.ProductId));
                var lineTotal = product.Price * item.Quantity;
                product.StockQty -= item.Quantity;
                _dbContext.OrderHistory.Add(new OrderHistory
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id.ToString(),
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal,
                    Image = product.Image ?? string.Empty,
                    Address = NormalizeOrderType(request.OrderType) == "Pickup"
                        ? string.Empty
                        : request.Address.Trim(),
                    OrderType = NormalizeOrderType(request.OrderType),
                    PaymentMethod = "Razorpay",
                    PaymentStatus = "Paid",
                    TransactionReference = request.RazorpayPaymentId,
                    RazorpayOrderId = request.RazorpayOrderId,
                    ServiceFeeAmount = Convert.ToSingle(serviceFee),
                    ChargedAmount = Convert.ToSingle(chargedAmount),
                    UserId = userId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();

            return new SalesOrderVerifyResponse
            {
                Success = true,
                Message = "Order placed successfully."
            };
        }

        private static void ValidateSalesOrderRequest(SalesOrderCheckoutRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new InvalidOperationException("Add at least one item to the cart.");
            }

            var orderType = NormalizeOrderType(request.OrderType);
            if (orderType == "Delivery" && string.IsNullOrWhiteSpace(request.Address))
            {
                throw new InvalidOperationException("Delivery address is required.");
            }

            foreach (var item in request.Items)
            {
                if (!Guid.TryParse(item.ProductId, out _))
                {
                    throw new InvalidOperationException("Invalid item selected.");
                }

                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException("Item quantity must be greater than zero.");
                }
            }
        }

        private async Task<decimal> CalculateSubtotalAsync(List<SalesOrderItemRequest> items)
        {
            var productIds = items.Select(x => Guid.Parse(x.ProductId)).ToList();
            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(x => productIds.Contains(x.Id) && !x.IsDeleted && x.Status && x.StockQty > 0)
                .ToListAsync();

            if (products.Count != items.Count)
            {
                throw new InvalidOperationException("One or more selected items are unavailable.");
            }

            return items.Sum(item =>
            {
                var product = products.First(x => x.Id == Guid.Parse(item.ProductId));
                if (product.StockQty < item.Quantity)
                {
                    throw new InvalidOperationException($"{product.Name} has only {product.StockQty} in stock.");
                }

                return Convert.ToDecimal(product.Price) * item.Quantity;
            });
        }

        private async Task<decimal> GetCheckoutServiceFeeAmountAsync(decimal amount)
        {
            var setting = await _dbContext.LoanSetting.AsNoTracking().FirstOrDefaultAsync();
            var fixedCharge = Math.Max(0, setting?.BookingPaymentFixedCharge ?? 0);
            var percentageCharge = Math.Clamp(setting?.BookingPaymentPercentageCharge ?? 0, 0, 100);
            return Math.Round(fixedCharge + (amount * percentageCharge / 100), 2, MidpointRounding.AwayFromZero);
        }

        private HttpClient CreateRazorpayClient(string keyId, string keySecret)
        {
            var client = _httpClientFactory.CreateClient("RazorpayClient");
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            return client;
        }

        private static string ComputeSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        }

        private static long ToPaise(decimal amount)
        {
            return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
        }

        private static string NormalizeOrderType(string value)
        {
            return string.Equals(value, "Pickup", StringComparison.OrdinalIgnoreCase)
                ? "Pickup"
                : "Delivery";
        }
    }
}
