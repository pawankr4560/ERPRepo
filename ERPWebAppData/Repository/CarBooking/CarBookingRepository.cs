using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public class CarBookingRepository : ICarBookingRepository
    {
        private readonly WebAppDbContext _context;

        public CarBookingRepository(WebAppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Vehicle>> GetVehiclesAsync()
        {
            return await _context.Vehicles.ToListAsync();
        }

        public async Task<List<CarBooking>> GetBookingsAsync()
        {
            return await _context.CarBookings.ToListAsync();
        }

        public async Task InsertBookingAsync(CarBooking booking)
        {
            if (booking == null) throw new ArgumentNullException(nameof(booking));
            await _context.CarBookings.AddAsync(booking);
        }

        public async Task InsertVehicleAsync(Vehicle vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
            await _context.Vehicles.AddAsync(vehicle);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
