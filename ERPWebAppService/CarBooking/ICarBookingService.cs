using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppModels.CarBooking;

namespace WebApp.Service.CarBooking
{
    public interface ICarBookingService
    {
        Task<List<VehicleDto>> GetVehiclesAsync();
        Task<List<CarBookingDto>> GetBookingsAsync();
        Task<CarBookingDto> CreateBookingAsync(string userId, CreateCarBookingDto dto);
        Task<VehicleDto> CreateVehicleAsync(VehicleDto dto);
    }
}
