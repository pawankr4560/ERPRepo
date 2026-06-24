using ERPWebAppModels.Booking;
using CarEntity = ERPWebAppData.Entity.Car;
using CategoryEntity = ERPWebAppData.Entity.Category;

namespace ERPWebAppService.Booking.Car;

public interface ICarService
{
    Task<IEnumerable<CarEntity>> GetAllAsync();
    Task<IEnumerable<CategoryEntity>> GetCategoriesAsync();
    Task<CarEntity?> GetByIdAsync(int id);
    Task<CarEntity> CreateAsync(CreateCarDto dto);
    Task<CarEntity?> UpdateAsync(int id, CreateCarDto dto);
    Task<bool> DeleteAsync(int id);
}
