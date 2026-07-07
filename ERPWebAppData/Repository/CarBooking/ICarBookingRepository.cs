using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public interface ICarBookingRepository
    {
        Task<List<Vehicle>> GetVehiclesAsync();
        Task<List<CarBooking>> GetBookingsAsync();
        Task InsertBookingAsync(CarBooking booking);
        Task InsertVehicleAsync(Vehicle vehicle);
        Task SaveAsync();
    }
}
