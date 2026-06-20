using ERPWebAppModels.Booking;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using CarEntity = ERPWebAppData.Entity.Car;

namespace ERPWebAppService.Booking.Car;

public class CarService : ICarService
{
    private readonly WebAppDbContext _context;

    public CarService(WebAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CarEntity>> GetAllAsync()
    {
        return await _context.Cars
            .Include(x => x.Category)
            .ToListAsync();
    }

    public async Task<CarEntity?> GetByIdAsync(int id)
    {
        return await _context.Cars
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CarEntity> CreateAsync(CreateCarDto dto)
    {
        var car = new CarEntity
        {
            Brand = dto.Brand,
            Model = dto.Model,
            Year = dto.Year,
            CategoryId = dto.CategoryId,
            Transmission = dto.Transmission,
            FuelType = dto.FuelType,
            Seats = dto.Seats,
            PricePerDay = dto.PricePerDay,
            ImageUrl = dto.ImageUrl,
            Status = "Available"
        };

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();

        return car;
    }

    public async Task<CarEntity?> UpdateAsync(int id, CreateCarDto dto)
    {
        var car = await _context.Cars.FindAsync(id);

        if (car is null)
        {
            return null;
        }

        car.Brand = dto.Brand;
        car.Model = dto.Model;
        car.Year = dto.Year;
        car.CategoryId = dto.CategoryId;
        car.Transmission = dto.Transmission;
        car.FuelType = dto.FuelType;
        car.Seats = dto.Seats;
        car.PricePerDay = dto.PricePerDay;
        car.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync();

        return car;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var car = await _context.Cars.FindAsync(id);

        if (car is null)
        {
            return false;
        }

        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();

        return true;
    }
}
