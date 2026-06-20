using ERPWebAppModels.Booking;
using CarEntity = ERPWebAppData.Entity.Car;

namespace ERPWebAppService.Booking.Car;

public interface ICarService
{
    Task<IEnumerable<CarEntity>> GetAllAsync();
    Task<CarEntity?> GetByIdAsync(int id);
    Task<CarEntity> CreateAsync(CreateCarDto dto);
    Task<CarEntity?> UpdateAsync(int id, CreateCarDto dto);
    Task<bool> DeleteAsync(int id);
}
