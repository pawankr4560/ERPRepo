using AutoMapper;
using MongoDB.Driver;
using WebApp.Data;
using WebApp.Model.Product;

namespace WebApp.Service.Product;

public class ProductService : IProductService
{
    private readonly MongoDbContext _context;
    private readonly IMapper _mapper;

    public ProductService(MongoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Data.Entity.Product>> ProductList() =>
        await _context.Products.Find(x => !x.IsDeleted).ToListAsync();

    public async Task<bool> Add(CreateProductRequestModel model)
    {
        var product = _mapper.Map<Data.Entity.Product>(model);
        product.Id = Guid.NewGuid();
        product.CreatedOn = DateTime.UtcNow;

        if (model.ProfileImage != null)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "images");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ProfileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await model.ProfileImage.CopyToAsync(fileStream);
            product.Image = Path.Combine("uploads", "images", uniqueFileName).Replace("\\", "/");
        }

        await _context.Products.InsertOneAsync(product);
        return true;
    }

    public async Task<Data.Entity.Product> Update(UpdateProductModel model)
    {
        var id = Guid.Parse(model.Id);
        var existing = await _context.Products
            .Find(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No data found.");

        _mapper.Map(model, existing);
        existing.Id = id;
        await _context.Products.ReplaceOneAsync(x => x.Id == id, existing);
        return existing;
    }

    public async Task<bool> Delete(Guid id)
    {
        var result = await _context.Products.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            Builders<Data.Entity.Product>.Update.Set(x => x.IsDeleted, true));
        return result.ModifiedCount > 0;
    }
}
