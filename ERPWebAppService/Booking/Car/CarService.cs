using ERPWebAppModels.Booking;
using MongoDB.Driver;
using WebApp.Data;
using CarEntity = ERPWebAppData.Entity.Car;

namespace ERPWebAppService.Booking.Car;

public class CarService : ICarService
{
    private static readonly string[] ValidTransmissions = ["Automatic", "Manual"];
    private static readonly string[] ValidFuelTypes = ["Petrol", "Diesel", "Electric", "Hybrid", "CNG"];
    private static readonly string[] ValidStatuses = ["Available", "Booked", "Maintenance", "Inactive"];
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;

    public CarService(MongoDbContext context, IMongoSequenceService sequences)
    {
        _context = context;
        _sequences = sequences;
    }

    public async Task<IEnumerable<CarEntity>> GetAllAsync()
    {
        var cars = await _context.Cars.Find(Builders<CarEntity>.Filter.Empty).ToListAsync();
        await AttachCategoriesAsync(cars);
        return cars;
    }

    public async Task<CarEntity?> GetByIdAsync(int id)
    {
        var car = await _context.Cars.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (car != null) await AttachCategoriesAsync([car]);
        return car;
    }

    public async Task<CarEntity> CreateAsync(CreateCarDto dto)
    {
        await ValidateAsync(dto);
        var car = Map(dto);
        car.Id = await _sequences.GetNextAsync("Cars");
        await _context.Cars.InsertOneAsync(car);
        return (await GetByIdAsync(car.Id))!;
    }

    public async Task<CarEntity?> UpdateAsync(int id, CreateCarDto dto)
    {
        if (!await _context.Cars.Find(x => x.Id == id).AnyAsync()) return null;
        await ValidateAsync(dto);
        var car = Map(dto);
        car.Id = id;
        await _context.Cars.ReplaceOneAsync(x => x.Id == id, car);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (await _context.Bookings.Find(x => x.CarId == id).AnyAsync())
            throw new InvalidOperationException(
                "This car has booking history and cannot be deleted. Set it to Inactive instead.");
        return (await _context.Cars.DeleteOneAsync(x => x.Id == id)).DeletedCount > 0;
    }

    private async Task ValidateAsync(CreateCarDto dto)
    {
        if (!await _context.Categories.Find(x => x.Id == dto.CategoryId).AnyAsync())
            throw new InvalidOperationException("The selected car category does not exist. Refresh the page and select a category.");
        if (!ValidTransmissions.Contains(dto.Transmission, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid transmission type.");
        if (!ValidFuelTypes.Contains(dto.FuelType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid fuel type.");
        if (!ValidStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid car status.");
    }

    private async Task AttachCategoriesAsync(IEnumerable<CarEntity> cars)
    {
        var list = cars.ToList();
        var ids = list.Select(x => x.CategoryId).Distinct().ToList();
        var categories = await _context.Categories
            .Find(Builders<ERPWebAppData.Entity.Category>.Filter.In(x => x.Id, ids)).ToListAsync();
        var byId = categories.ToDictionary(x => x.Id);
        foreach (var car in list) car.Category = byId.GetValueOrDefault(car.CategoryId);
    }

    private static CarEntity Map(CreateCarDto dto) => new()
    {
        Brand = dto.Brand.Trim(), Model = dto.Model.Trim(), Year = dto.Year,
        CategoryId = dto.CategoryId, Transmission = Canonical(dto.Transmission, ValidTransmissions),
        FuelType = Canonical(dto.FuelType, ValidFuelTypes), Seats = dto.Seats,
        PricePerDay = dto.PricePerDay,
        ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
        Status = Canonical(dto.Status, ValidStatuses)
    };

    private static string Canonical(string value, string[] validValues) =>
        validValues.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
}
