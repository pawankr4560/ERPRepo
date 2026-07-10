using ERPWebAppModels.Construction;
using WebApp.Data;
using Microsoft.EntityFrameworkCore;
using ERPWebAppData.Entity;
using ERPWebAppModels.Menu;
using ERPWebApp.Server.Enum;
using System.Globalization;

namespace ERPWebAppService.Construction
{
    public class ConstructionService : IConstructionService
    {
        private readonly WebAppDbContext dbContext;

        public ConstructionService(WebAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ConstructionDashboardDto> DashbordData(string userId)
        {
            try
            {
                return new ConstructionDashboardDto
                {
                    TotalProducts = 48,

                    PendingQuotes = await dbContext.ConstructionQuotes
                 .CountAsync(x => x.Status == 0 && x.UserId == userId),

                    ActiveOrders = await dbContext.ConstructionQuotes
                 .CountAsync(x => x.Status == 1 && x.UserId == userId),

                    DeliveriesToday = 2,

                    RecentActivities = new List<ConstructionActivityDto>
            {
                new ConstructionActivityDto(
                    "act_001",
                    "QUOTE",
                    "Quote requested for TMT Steel",
                    "Supplier confirmation pending",
                    DateTime.Parse("2026-07-08T10:30:00Z").ToUniversalTime())
            }
                };
            }
            catch { throw; }
        }

        public async Task<ConstructionProductListDto> GetProducts(
        int? categoryId,
        string? search,
        int page = 1,
        int limit = 20)
        {
            page = Math.Max(1, page);
            limit = Math.Clamp(limit, 1, 100);

            var query =
                from product in dbContext.Products.AsNoTracking()
                join category in dbContext.Categories.AsNoTracking()
                    on product.CategoryId equals category.Id
                join subCategory in dbContext.SubCategory.AsNoTracking()
                    on product.SubcategoryId equals subCategory.Id
                join unit in dbContext.UnitOfMeasure.AsNoTracking()
                    on product.UnitId equals unit.UOMIndex
                where product.IsDeleted == false && product.IsActive == true
                select new
                {
                    Product = product,
                    CategoryName = category.Name,
                    SubCategoryName = subCategory.Name,
                    UnitName = unit.UOMName
                };

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(x => x.Product.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Product.Name.Contains(search) ||
                    x.CategoryName.Contains(search) ||
                    x.SubCategoryName.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Product.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(x => new ConstructionProductDto
                {
                    Id = x.Product.Id.ToString(),
                    Name = x.Product.Name,
                    CategoryId = x.Product.SubcategoryId,
                    CategoryName = x.SubCategoryName,
                    Rate = x.Product.Price,
                    UnitIndex = x.Product.UnitId,
                    Unit = x.UnitName
                })
                .ToListAsync();

            return new ConstructionProductListDto
            {
                Items = items,
                Pagination = new PaginationDto
                {
                    Page = page,
                    Limit = limit,
                    Total = total
                }
            };
        }

        public async Task<List<UnitDto>> Units()
        {
            return await dbContext.UnitOfMeasure
                .AsNoTracking()
                .OrderBy(x => x.UOMName)
                .Select(x => new UnitDto
                {
                    Id = x.UOMIndex,
                    Code = x.UOMCode,
                    Name = x.UOMName
                })
                .ToListAsync();
        }

        public async Task<ConstructionQuoteResponseDto> CreateQuote(
    ConstructionQuoteRequest request,string userId)
        {
            if (request == null)
                throw new Exception("Request body is required.");

            //var product = await dbContext.Products
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(x => x.Id.ToString() == request.ProductId);

            //if (product == null)
            //    throw new Exception("Product not found.");

            var quote = new ConstructionQuote
            {
                ProductId = request.ProductId,
                UserId = userId,
                Quantity = request.Quantity,
                UnitId = request.UnitId,
                DeliveryLocation = request.DeliveryLocation,
                RequiredDate = DateTime.UtcNow,
                Status = 0, // Pending
                EstimatedAmount = request.EstimatedAmount,
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.ConstructionQuotes.AddAsync(quote);
            await dbContext.SaveChangesAsync();

            return new ConstructionQuoteResponseDto
            {
                QuoteId = quote.Id,
                Status = "Pending",
                ProductId = request.ProductId,
                CategoryId=request.CategoryId,
                Quantity = quote.Quantity,
                UnitId = quote.UnitId,
                DeliveryLocation = quote.DeliveryLocation,
                RequiredDate = quote.RequiredDate,
                CreatedAt = quote.CreatedAt
            };
        }

        public async Task<List<ConstructionQuoteDto>> GetQuotes(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is required.");

            var quotes = await (
                from quote in dbContext.ConstructionQuotes.AsNoTracking()

                join product in dbContext.Products.AsNoTracking()
                    on quote.ProductId equals product.Id

                join category in dbContext.Categories.AsNoTracking()
                    on product.CategoryId equals category.Id

                join unit in dbContext.UnitOfMeasure.AsNoTracking()
                    on quote.UnitId equals unit.UOMIndex

                where quote.UserId == userId

                orderby quote.CreatedAt descending

                select new ConstructionQuoteDto
                {
                    QuoteId = quote.Id,

                    Status = quote.Status == 0
                        ? "Pending"
                        : quote.Status == 1
                            ? "Approved"
                            : quote.Status == 2
                                ? "Rejected"
                                : quote.Status == 3
                                    ? "Completed"
                                    : "Unknown",

                    ProductName = product.Name,
                    CategoryName = category.Name,
                    Quantity = quote.Quantity,

                    UnitId = quote.UnitId,
                    UnitName = unit.UOMName,

                    EstimatedAmount = quote.EstimatedAmount,
                    FinalQuotedAmount = quote.FinalQuotedAmount,

                    DeliveryLocation = quote.DeliveryLocation,
                    RequiredDate = quote.RequiredDate,
                    CreatedAt = quote.CreatedAt
                }
            ).ToListAsync();

            return quotes;
        }

        public async Task<List<CategorieDto>> GetCategories()
        {
            return await dbContext.SubCategory
                .AsNoTracking()
                .Where(x=>x.CategoryId==2)
                .Select(x => new CategorieDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    subtitle = x.Description
                })
                .ToListAsync();
        }

        public async Task<UpdateQuotePriceResponseDto> UpdateQuotePrice(
    int quoteId,
    UpdateQuotePriceRequest request)
        {
            if (quoteId <= 0)
                throw new ArgumentException("A valid quote ID is required.");

            if (request == null)
                throw new ArgumentNullException(
                    nameof(request),
                    "Request body is required.");

            if (request.FinalQuotedAmount <= 0)
                throw new ArgumentException(
                    "Final quoted amount must be greater than zero.");

            var quote = await dbContext.ConstructionQuotes
                .FirstOrDefaultAsync(x => x.Id == quoteId);

            if (quote == null)
                throw new KeyNotFoundException("Construction quote not found.");

            // Optional: prevent updating completed/cancelled quotes
            if (quote.Status == (int)ConstructionQuoteStatus.Completed)
            {
                throw new InvalidOperationException(
                    "The price of a completed quote cannot be updated.");
            }

            if (quote.Status == (int)ConstructionQuoteStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "The price of a cancelled quote cannot be updated.");
            }

            var updatedAt = DateTime.UtcNow;

            quote.FinalQuotedAmount = request.FinalQuotedAmount;
            quote.Status = (int)ConstructionQuoteStatus.PriceShared;

            await dbContext.SaveChangesAsync();

            return new UpdateQuotePriceResponseDto
            {
                QuoteId = quote.Id,
                Status = ConstructionQuoteStatus.PriceShared.ToString(),
                EstimatedAmount = quote.EstimatedAmount,
                FinalQuotedAmount = quote.FinalQuotedAmount,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        public async Task<CreateConstructionOrderResponseDto> CreateOrderFromQuote(int quoteId,string userId)
        {
            if (quoteId <= 0)
                throw new ArgumentException("A valid quote ID is required.");

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is required.");

            var quote = await dbContext.ConstructionQuotes
                .FirstOrDefaultAsync(x =>
                    x.Id == quoteId &&
                    x.UserId == userId);

            if (quote == null)
                throw new KeyNotFoundException("Construction quote not found.");

            if (quote.Status == (int)ConstructionQuoteStatus.Rejected)
                throw new InvalidOperationException(
                    "An order cannot be created from a rejected quote.");

            if (quote.Status == (int)ConstructionQuoteStatus.Cancelled)
                throw new InvalidOperationException(
                    "An order cannot be created from a cancelled quote.");

            if (quote.Status == (int)ConstructionQuoteStatus.Completed)
                throw new InvalidOperationException(
                    "An order cannot be created from a completed quote.");

            if (quote.FinalQuotedAmount <= 0)
                throw new InvalidOperationException(
                    "The final quote price has not been shared.");

            var orderAlreadyExists = await dbContext.ConstructionOrders
                .AsNoTracking()
                .AnyAsync(x =>
                    x.QuoteId == quoteId);

            if (orderAlreadyExists)
                throw new InvalidOperationException(
                    "An order has already been placed for this quote.");

            var product = await dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == quote.ProductId &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            var unit = await dbContext.UnitOfMeasure
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UOMIndex == quote.UnitId);

            if (unit == null)
                throw new KeyNotFoundException("Unit of measure not found.");

            var currentUtcDate = DateTime.UtcNow;

            await using var transaction =
                await dbContext.Database.BeginTransactionAsync();

            try
            {
                var order = new ConstructionOrder
                {
                    QuoteId = quote.Id,
                    ProductId = quote.ProductId,
                    UserId = userId,
                    Quantity = quote.Quantity,
                    UnitId = quote.UnitId,
                    TotalAmount = quote.FinalQuotedAmount,
                    DeliveryLocation = quote.DeliveryLocation,
                    DeliveryDate = quote.RequiredDate,
                    Status = (int)ConstructionOrderStatus.Placed,
                    CreatedAt = currentUtcDate
                };

                await dbContext.ConstructionOrders.AddAsync(order);

                quote.Status = (int)ConstructionQuoteStatus.OrderPlaced;
                //quote.UpdatedAt = currentUtcDate;

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateConstructionOrderResponseDto
                {
                    OrderId = order.Id,
                    QuoteId = quote.Id,
                    Status = ConstructionOrderStatus.Placed.ToString(),
                    ProductName = product.Name,
                    Quantity = order.Quantity,
                    UnitId = order.UnitId,
                    UnitName = unit.UOMName,
                    TotalAmount = order.TotalAmount,
                    DeliveryLocation = order.DeliveryLocation,
                    DeliveryDate = order.DeliveryDate,
                    CreatedAt = order.CreatedAt
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ConstructionOrderDto>> GetOrders(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is required.");

            var orders = await (
                from order in dbContext.ConstructionOrders.AsNoTracking()

                join product in dbContext.Products.AsNoTracking()
                    on order.ProductId equals product.Id

                join unit in dbContext.UnitOfMeasure.AsNoTracking()
                    on order.UnitId equals unit.UOMIndex

                where order.UserId == userId

                orderby order.CreatedAt descending

                select new ConstructionOrderDto
                {
                    OrderId = order.Id,
                    QuoteId = (int)order.QuoteId,

                    Status = order.Status == (int)ConstructionOrderStatus.Placed
                        ? nameof(ConstructionOrderStatus.Placed)
                        : order.Status == (int)ConstructionOrderStatus.Confirmed
                            ? nameof(ConstructionOrderStatus.Confirmed)
                            : order.Status == (int)ConstructionOrderStatus.Processing
                                ? nameof(ConstructionOrderStatus.Processing)
                                : order.Status == (int)ConstructionOrderStatus.Shipped
                                    ? nameof(ConstructionOrderStatus.Shipped)
                                    : order.Status == (int)ConstructionOrderStatus.Delivered
                                        ? nameof(ConstructionOrderStatus.Delivered)
                                        : order.Status == (int)ConstructionOrderStatus.Cancelled
                                            ? nameof(ConstructionOrderStatus.Cancelled)
                                            : "Unknown",

                    ProductName = product.Name,
                    Quantity = order.Quantity,
                    UnitId = order.UnitId,
                    UnitName = unit.UOMName,
                    TotalAmount = order.TotalAmount,
                    DeliveryLocation = order.DeliveryLocation,
                    DeliveryDate = order.DeliveryDate,
                    CreatedAt = order.CreatedAt
                }
            ).ToListAsync();

            return orders;
        }

        public async Task<List<ConstructionDeliveryDto>> GetDeliveries(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User ID is required.");

            var deliveries = await (
                from delivery in dbContext.ConstructionDelivery.AsNoTracking()

                join order in dbContext.ConstructionOrders.AsNoTracking()
                    on delivery.OrderId equals order.Id

                join product in dbContext.Products.AsNoTracking()
                    on order.ProductId equals product.Id

                where order.UserId == userId
                      && !delivery.IsDeleted

                orderby delivery.CreatedAt descending

                select new
                {
                    Delivery = delivery,
                    ProductName = product.Name
                }
            ).ToListAsync();

            return deliveries.Select(x => new ConstructionDeliveryDto
            {
                DeliveryId = x.Delivery.Id,
                OrderId = x.Delivery.OrderId,
                ProductName = x.ProductName,
                VehicleNumber = x.Delivery.VehicleNumber,
                DriverName = x.Delivery.DriverName,
                Status = GetDeliveryStatusName(x.Delivery.Status),
                Eta = FormatEta(x.Delivery.EstimatedArrivalTime),
                Progress = NormalizeProgress(x.Delivery.Progress)
            }).ToList();
        }
        private static string GetDeliveryStatusName(int status)
        {
            return status switch
            {
                (int)ConstructionDeliveryStatus.Preparing => "Preparing",
                (int)ConstructionDeliveryStatus.Dispatched => "Dispatched",
                (int)ConstructionDeliveryStatus.OutForDelivery => "Out for Delivery",
                (int)ConstructionDeliveryStatus.Delivered => "Delivered",
                (int)ConstructionDeliveryStatus.Delayed => "Delayed",
                (int)ConstructionDeliveryStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        private static string? FormatEta(DateTime? eta)
        {
            if (!eta.HasValue)
                return null;

            var etaDate = eta.Value;
            var today = DateTime.UtcNow.Date;

            if (etaDate.Date == today)
                return $"Today, {etaDate.ToString("h:mm tt", CultureInfo.InvariantCulture)}";

            if (etaDate.Date == today.AddDays(1))
                return $"Tomorrow, {etaDate.ToString("h:mm tt", CultureInfo.InvariantCulture)}";

            return etaDate.ToString(
                "dd MMM yyyy, h:mm tt",
                CultureInfo.InvariantCulture);
        }

        private static decimal NormalizeProgress(decimal progress)
        {
            return Math.Clamp(progress, 0m, 1m);
        }
    }
}
