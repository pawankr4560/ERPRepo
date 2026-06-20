using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Model.Product;

namespace WebApp.Service.Product
{
    public class ProductService : IProductService
    {
        private readonly WebAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ProductService(WebAppDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Data.Entity.Product>> ProductList()
        {
            try
            {
                return await _dbContext.Products.Where(x=>!x.IsDeleted).ToListAsync();
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> Add(CreateProductRequestModel model)
        {
            try
            {
                var reuqest = _mapper.Map<Data.Entity.Product>(model);
                reuqest.Id = Guid.NewGuid();
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
                return true;
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
                return true;
            }
            catch (Exception) { throw; }
        }
    }
}
