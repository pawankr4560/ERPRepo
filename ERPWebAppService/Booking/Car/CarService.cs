using ERPWebAppModels.Booking;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using CarEntity = ERPWebAppData.Entity.Car;
using CategoryEntity = ERPWebAppData.Entity.Category;

namespace ERPWebAppService.Booking.Car;

public class CarService : ICarService
{
    private const string CarCategoryListCacheKey = "car_category_list";
    private static readonly DistributedCacheEntryOptions CarCategoryListCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private static readonly string[] ValidTransmissions = ["Automatic", "Manual"];
    private static readonly string[] ValidFuelTypes =
        ["Petrol", "Diesel", "Electric", "Hybrid", "CNG"];
    private static readonly string[] ValidStatuses =
        ["Available", "Booked", "Maintenance", "Inactive"];

    private readonly WebAppDbContext _context;
    private readonly IDistributedCache _cache;

    public CarService(WebAppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<CarEntity>> GetAllAsync()
    {
        return await _context.Cars
            .Include(x => x.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoryEntity>> GetCategoriesAsync()
    {
        string? cachedCategories = null;

        try
        {
            cachedCategories = await _cache.GetStringAsync(CarCategoryListCacheKey);
        }
        catch
        {
            cachedCategories = null;
        }

        if (!string.IsNullOrWhiteSpace(cachedCategories))
        {
            return JsonSerializer.Deserialize<List<CategoryEntity>>(cachedCategories)
                ?? [];
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();

        try
        {
            await _cache.SetStringAsync(
                CarCategoryListCacheKey,
                JsonSerializer.Serialize(categories),
                CarCategoryListCacheOptions);
        }
        catch
        {
            // Redis caching is best-effort; the database result is still valid.
        }

        return categories;
    }

    public async Task<CarEntity?> GetByIdAsync(int id)
    {
        return await _context.Cars
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CarEntity> CreateAsync(CreateCarDto dto)
    {
        await ValidateAsync(dto);

        var car = new CarEntity
        {
            Brand = dto.Brand.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            CategoryId = dto.CategoryId,
            Transmission = Canonical(dto.Transmission, ValidTransmissions),
            FuelType = Canonical(dto.FuelType, ValidFuelTypes),
            Seats = dto.Seats,
            PricePerDay = dto.PricePerDay,
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            Status = Canonical(dto.Status, ValidStatuses)
        };

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(car.Id))!;
    }

    public async Task<CarEntity?> UpdateAsync(int id, CreateCarDto dto)
    {
        var car = await _context.Cars.FindAsync(id);

        if (car is null)
        {
            return null;
        }

        await ValidateAsync(dto);

        car.Brand = dto.Brand.Trim();
        car.Model = dto.Model.Trim();
        car.Year = dto.Year;
        car.CategoryId = dto.CategoryId;
        car.Transmission = Canonical(dto.Transmission, ValidTransmissions);
        car.FuelType = Canonical(dto.FuelType, ValidFuelTypes);
        car.Seats = dto.Seats;
        car.PricePerDay = dto.PricePerDay;
        car.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        car.Status = Canonical(dto.Status, ValidStatuses);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(car.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var car = await _context.Cars.FindAsync(id);

        if (car is null)
        {
            return false;
        }

        if (await _context.Bookings.AnyAsync(x => x.CarId == id))
        {
            throw new InvalidOperationException(
                "This car has booking history and cannot be deleted. Set it to Inactive instead.");
        }

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateAsync(CreateCarDto dto)
    {
        if (!await _context.Categories.AnyAsync(x => x.Id == dto.CategoryId))
        {
            throw new InvalidOperationException(
                "The selected car category does not exist. Refresh the page and select a category.");
        }

        if (!ValidTransmissions.Contains(
            dto.Transmission,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid transmission type.");
        }

        if (!ValidFuelTypes.Contains(dto.FuelType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid fuel type.");
        }

        if (!ValidStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid car status.");
        }
    }

    private static string Canonical(string value, string[] validValues)
    {
        return validValues.First(item =>
            item.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
