using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Globalization;

namespace WebApp.Server.Controllers;

[Route("api/dairy")]
[ApiController]
[Authorize]
public class DairyController : ControllerBase
{
    private static int _productSequence = 1;
    private static int _collectionSequence = 1;
    private static int _saleSequence = 1;
    private static int _customerSequence = 1;
    private static int _paymentSequence = 1;

    private static readonly ConcurrentDictionary<string, DairyProductDto> Products = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, DairyCollectionDto> Collections = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, DairySaleDto> Sales = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, DairyCustomerDto> Customers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, DairyPaymentDto> Payments = new(StringComparer.OrdinalIgnoreCase);

    static DairyController()
    {
        Products.TryAdd("prod_001", new DairyProductDto("prod_001", "Milk", "L", 85m, 48m, 60m));
        Products.TryAdd("prod_002", new DairyProductDto("prod_002", "Paneer", "Kg", 12m, 260m, 340m));

        Collections.TryAdd("col_001", new DairyCollectionDto("col_001", "Rahul Sharma", "Cow", 20m, "L", 4.5m, 42m, 840m, "2026-07-08"));
        Collections.TryAdd("col_002", new DairyCollectionDto("col_002", "Amit Patel", "Buffalo", 30m, "L", 6.2m, 52m, 1560m, "2026-07-08"));

        Sales.TryAdd("sale_001", new DairySaleDto("sale_001", "Mohit Store", "Milk", 10m, "L", 60m, 600m, "Paid", "2026-07-08"));
        Sales.TryAdd("sale_002", new DairySaleDto("sale_002", "Sharma Sweets", "Paneer", 5m, "Kg", 340m, 1700m, "Pending", "2026-07-08"));

        Customers.TryAdd("cust_001", new DairyCustomerDto("cust_001", "Rahul Sharma", "9876543210", "Farmer", 2500m, "Village Road"));
        Customers.TryAdd("cust_002", new DairyCustomerDto("cust_002", "Mohit Store", "9876543211", "Customer", 0m, "Main Market"));

        Payments.TryAdd("pay_001", new DairyPaymentDto("pay_001", "Sharma Sweets", 4200m, "UPI", "Pending", "2026-07-08"));
        Payments.TryAdd("pay_002", new DairyPaymentDto("pay_002", "Rahul Sharma", 2500m, "Cash", "Received", "2026-07-08"));
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        var today = "2026-07-08";
        var todayCollection = Collections.Values.Where(item => item.CollectionDate == today).Sum(item => item.Quantity);
        var todaySales = Sales.Values.Where(item => item.SaleDate == today).Sum(item => item.Amount);
        var pendingPayment = Payments.Values.Where(item => item.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)).Sum(item => item.Amount);

        return Ok(new
        {
            success = true,
            data = new
            {
                todayCollection = new
                {
                    quantity = todayCollection,
                    unit = "L",
                    displayValue = $"{todayCollection:0.#} L"
                },
                todaySales = new
                {
                    amount = todaySales,
                    displayValue = FormatCurrency(todaySales)
                },
                pendingPayment = new
                {
                    amount = pendingPayment,
                    displayValue = FormatCurrency(pendingPayment)
                },
                productsCount = 15,
                recentActivities = new[]
                {
                    new
                    {
                        id = "act_001",
                        type = "COLLECTION",
                        title = "Milk collected",
                        subtitle = "Rahul Sharma - 20 L",
                        time = "5 mins ago",
                        createdAt = DateTime.Parse("2026-07-08T10:30:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                    }
                }
            }
        });
    }

    [HttpGet("products")]
    public IActionResult GetProducts([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var items = Paginate(FilterProducts(search), page, limit)
            .Select(ToProductResponse)
            .ToList();

        return Ok(new { success = true, data = new { items } });
    }

    [HttpPost("products")]
    public IActionResult AddProduct([FromBody] DairyProductRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var product = new DairyProductDto(
            $"prod_{Interlocked.Increment(ref _productSequence):000}",
            request.Name,
            request.Unit,
            request.Stock,
            request.PurchasePrice,
            request.SellingPrice);

        Products[product.Id] = product;

        return Ok(new
        {
            success = true,
            message = "Product added successfully",
            data = ToProductResponse(product)
        });
    }

    [HttpGet("collections")]
    public IActionResult GetCollections([FromQuery] string? search, [FromQuery] string? date, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var filtered = Collections.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(item => item.FarmerName.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(date))
            filtered = filtered.Where(item => item.CollectionDate == date);

        var list = filtered.ToList();
        var items = Paginate(list, page, limit).ToList();
        var totalQuantity = list.Sum(item => item.Quantity);
        var totalAmount = list.Sum(item => item.Amount);

        return Ok(new
        {
            success = true,
            data = new
            {
                summary = new
                {
                    todayCollection = $"{totalQuantity:0.#} L",
                    totalAmount = FormatCurrency(totalAmount)
                },
                items
            }
        });
    }

    [HttpPost("collections")]
    public IActionResult AddCollection([FromBody] DairyCollectionRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var amount = request.Quantity * request.Rate;
        var collection = new DairyCollectionDto(
            $"col_{Interlocked.Increment(ref _collectionSequence):000}",
            request.FarmerName,
            request.MilkType,
            request.Quantity,
            "L",
            request.Fat,
            request.Rate,
            amount,
            request.CollectionDate);

        Collections[collection.Id] = collection;

        return Ok(new
        {
            success = true,
            message = "Collection saved successfully",
            data = collection
        });
    }

    [HttpGet("sales")]
    public IActionResult GetSales([FromQuery] string? search, [FromQuery] string? date, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var filtered = Sales.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(item => item.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(date))
            filtered = filtered.Where(item => item.SaleDate == date);
        if (!string.IsNullOrWhiteSpace(status))
            filtered = filtered.Where(item => item.PaymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase));

        var list = filtered.ToList();
        var items = Paginate(list, page, limit).ToList();
        var totalSales = list.Sum(item => item.Amount);

        return Ok(new
        {
            success = true,
            data = new
            {
                summary = new
                {
                    todaySales = FormatCurrency(totalSales),
                    orders = list.Count
                },
                items
            }
        });
    }

    [HttpPost("sales")]
    public IActionResult AddSale([FromBody] DairySaleRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var product = Products.Values.FirstOrDefault(item => item.Name.Equals(request.ProductName, StringComparison.OrdinalIgnoreCase));
        var unit = product?.Unit ?? "L";
        var amount = request.Quantity * request.Rate;
        var sale = new DairySaleDto(
            $"sale_{Interlocked.Increment(ref _saleSequence):000}",
            request.CustomerName,
            request.ProductName,
            request.Quantity,
            unit,
            request.Rate,
            amount,
            request.PaymentStatus,
            request.SaleDate);

        Sales[sale.Id] = sale;

        return Ok(new
        {
            success = true,
            message = "Sale saved successfully",
            data = sale
        });
    }

    [HttpGet("customers")]
    public IActionResult GetCustomers([FromQuery] string? search, [FromQuery] string? type, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var filtered = Customers.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            filtered = filtered.Where(item => item.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(type))
            filtered = filtered.Where(item => item.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        var items = Paginate(filtered, page, limit).ToList();
        return Ok(new { success = true, data = new { items } });
    }

    [HttpPost("customers")]
    public IActionResult AddCustomer([FromBody] DairyCustomerRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var customer = new DairyCustomerDto(
            $"cust_{Interlocked.Increment(ref _customerSequence):000}",
            request.Name,
            request.Phone,
            request.Type,
            request.OpeningBalance,
            request.Address);

        Customers[customer.Id] = customer;

        return Ok(new
        {
            success = true,
            message = "Customer added successfully",
            data = customer
        });
    }

    [HttpGet("payments")]
    public IActionResult GetPayments([FromQuery] string? status, [FromQuery] string? date, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var filtered = Payments.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
            filtered = filtered.Where(item => item.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(date))
            filtered = filtered.Where(item => item.PaymentDate == date);

        var items = Paginate(filtered, page, limit).ToList();
        return Ok(new { success = true, data = new { items } });
    }

    [HttpPost("payments")]
    public IActionResult AddPayment([FromBody] DairyPaymentRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var payment = new DairyPaymentDto(
            $"pay_{Interlocked.Increment(ref _paymentSequence):000}",
            request.PartyName,
            request.Amount,
            request.Mode,
            request.Status,
            request.PaymentDate);

        Payments[payment.Id] = payment;

        return Ok(new
        {
            success = true,
            message = "Payment saved successfully",
            data = payment
        });
    }

    [HttpGet("reports")]
    public IActionResult GetReports([FromQuery] string period = "Today")
    {
        return Ok(new
        {
            success = true,
            data = new
            {
                period,
                reports = new[]
                {
                    new { key = "dailyCollection", title = "Daily Collection", value = "120 L", progress = 0.82m },
                    new { key = "salesReport", title = "Sales Report", value = "Rs. 8,500", progress = 0.74m },
                    new { key = "pendingPayments", title = "Pending Payments", value = "Rs. 12,000", progress = 0.52m }
                }
            }
        });
    }

    private static IEnumerable<DairyProductDto> FilterProducts(string? search)
    {
        var query = Products.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(item => item.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        return query;
    }

    private static IEnumerable<T> Paginate<T>(IEnumerable<T> source, int page, int limit)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);
        return source.Skip((page - 1) * limit).Take(limit);
    }

    private static object ToProductResponse(DairyProductDto product) => new
    {
        product.Id,
        product.Name,
        product.Unit,
        product.Stock,
        product.PurchasePrice,
        product.SellingPrice,
        displayStock = $"{product.Stock:0.#} {product.Unit}"
    };

    private static string FormatCurrency(decimal amount) =>
        string.Format(CultureInfo.InvariantCulture, "Rs. {0:N0}", amount);

    public record DairyProductDto(string Id, string Name, string Unit, decimal Stock, decimal PurchasePrice, decimal SellingPrice);
    public record DairyCollectionDto(string Id, string FarmerName, string MilkType, decimal Quantity, string Unit, decimal Fat, decimal Rate, decimal Amount, string CollectionDate);
    public record DairySaleDto(string Id, string CustomerName, string ProductName, decimal Quantity, string Unit, decimal Rate, decimal Amount, string PaymentStatus, string SaleDate);
    public record DairyCustomerDto(string Id, string Name, string Phone, string Type, decimal Balance, string Address);
    public record DairyPaymentDto(string Id, string PartyName, decimal Amount, string Mode, string Status, string PaymentDate);

    public class DairyProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Stock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
    }

    public class DairyCollectionRequest
    {
        public string FarmerName { get; set; } = string.Empty;
        public string MilkType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Fat { get; set; }
        public decimal Rate { get; set; }
        public string CollectionDate { get; set; } = string.Empty;
    }

    public class DairySaleRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string SaleDate { get; set; } = string.Empty;
    }

    public class DairyCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    public class DairyPaymentRequest
    {
        public string PartyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Mode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentDate { get; set; } = string.Empty;
    }
}
