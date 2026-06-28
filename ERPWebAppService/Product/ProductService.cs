using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Model.Product;

namespace WebApp.Service.Product
{
    public class ProductService : IProductService
    {
        private const string ProductListCacheKey = "product_list";
        private static readonly Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions ProductListCacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        private readonly WebAppDbContext _dbContext;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;
        private readonly IMapper _mapper;

        public ProductService(WebAppDbContext dbContext,
            Microsoft.Extensions.Caching.Distributed.IDistributedCache cache,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Data.Entity.Product>> ProductList()
        {
            try
            {
                string? cachedProducts = null;

                try
                {
                    cachedProducts = await Microsoft.Extensions.Caching.Distributed.DistributedCacheExtensions
                        .GetStringAsync(_cache, ProductListCacheKey);
                }
                catch
                {
                    cachedProducts = null;
                }

                if (!string.IsNullOrWhiteSpace(cachedProducts))
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<Data.Entity.Product>>(cachedProducts)
                        ?? [];
                }

                var products = await _dbContext.Products.Where(x=>!x.IsDeleted).ToListAsync();

                try
                {
                    await Microsoft.Extensions.Caching.Distributed.DistributedCacheExtensions
                        .SetStringAsync(
                            _cache,
                            ProductListCacheKey,
                            System.Text.Json.JsonSerializer.Serialize(products),
                            ProductListCacheOptions);
                }
                catch
                {
                    // Redis caching is best-effort; the database result is still valid.
                }

                return products;
            }
            catch (Exception) { throw; }
        }

        public async Task<Data.Entity.Product> Add(CreateProductRequestModel model)
        {
            try
            {
                var reuqest = _mapper.Map<Data.Entity.Product>(model);
                reuqest.Id = Guid.NewGuid();
                reuqest.CreatedOn = model.CreatedOn == default ? DateTime.UtcNow : model.CreatedOn;
                reuqest.IsActive = model.IsActive || model.Status;
                reuqest.IsDeleted = false;

                if (model.ProfileImage != null)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "uploads", "images");
                    Directory.CreateDirectory(uploadsFolder); 

                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ProfileImage.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(fileStream);
                    }

                    reuqest.Image = Path.Combine("uploads", "images", uniqueFileName).Replace("\\", "/");
                }

                await _dbContext.Products.AddAsync(reuqest);
                await _dbContext.SaveChangesAsync();
                await RemoveProductListCacheAsync();
                return reuqest;
            }
            catch (Exception) { throw; }
        }

        public async Task<Data.Entity.Product> Update(UpdateProductModel model)
        {
            try
            {
                var data = _mapper.Map<Data.Entity.Product>(model);
                data.Id = Guid.Parse(model.Id);
                _dbContext.Products.Update(data);
                await _dbContext.SaveChangesAsync();
                await RemoveProductListCacheAsync();
                return data;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> Delete(Guid id)
        {
            try
            {
                var data = await _dbContext.Products.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (data == null)
                    throw new Exception("No data found.");
                data.IsDeleted = true;
                _dbContext.Products.Update(data);
                await _dbContext.SaveChangesAsync();
                await RemoveProductListCacheAsync();
                return true;
            }
            catch (Exception) { throw; }
        }

        private async Task RemoveProductListCacheAsync()
        {
            try
            {
                await _cache.RemoveAsync(ProductListCacheKey);
            }
            catch
            {
                // Redis caching is best-effort; product changes are already saved.
            }
        }
    }
}
