using ERPWebAppModels.Construction;
using WebApp.Data;
using Microsoft.EntityFrameworkCore;
using ERPWebAppData.Entity;

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

        public async Task<ConstructionQuoteResponseDto> CreateQuote(
    ConstructionQuoteRequest request,string userId)
        {
            if (request == null)
                throw new Exception("Request body is required.");

            var product = await dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id.ToString() == request.ProductId);

            if (product == null)
                throw new Exception("Product not found.");

            var quote = new ConstructionQuote
            {
                ProductName = product.Id.ToString(),
                UserId = userId,
                Quantity = request.Quantity,
                //UnitIndex = request.Unit,
                DeliveryLocation = request.DeliveryLocation,
                RequiredDate = DateTime.UtcNow,
                Status = 0, // Pending
                TotalAmount = (decimal)(product.Price * request.Quantity),
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.ConstructionQuotes.AddAsync(quote);
            await dbContext.SaveChangesAsync();

            return new ConstructionQuoteResponseDto
            {
                QuoteId = quote.Id,
                Status = "Pending",
                ProductName = product.Name,
                Quantity = quote.Quantity,
                //Unit = quote.Unit,
                DeliveryLocation = quote.DeliveryLocation,
                RequiredDate = quote.RequiredDate,
                CreatedAt = quote.CreatedAt
            };
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
    }
}
