using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPWebAppData.Entity;
using WebApp.Data.Repository;
using ERPWebAppModels.CarBooking;
using WebApp.Service.Dashboard;

namespace WebApp.Service.CarBooking
{
    public class CarBookingService : ICarBookingService
    {
        private readonly ICarBookingRepository _carBookingRepo;
        private readonly IDashboardService _dashboardService;

        public CarBookingService(ICarBookingRepository carBookingRepo, IDashboardService dashboardService)
        {
            _carBookingRepo = carBookingRepo ?? throw new ArgumentNullException(nameof(carBookingRepo));
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        public async Task<List<VehicleDto>> GetVehiclesAsync()
        {
            var vehicles = await _carBookingRepo.GetVehiclesAsync();
            return vehicles.Select(v => new VehicleDto
            {
                Id = v.Id,
                Model = v.Model,
                RegistrationNumber = v.RegistrationNumber,
                DailyRate = v.DailyRate,
                Status = v.Status
            }).ToList();
        }

        public async Task<List<CarBookingDto>> GetBookingsAsync()
        {
            var bookings = await _carBookingRepo.GetBookingsAsync();
            return bookings.Select(b => new CarBookingDto
            {
                Id = b.Id,
                CustomerName = b.CustomerName,
                CarModel = b.CarModel,
                RegistrationNumber = b.RegistrationNumber,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                DailyRate = b.DailyRate,
                Status = b.Status
            }).ToList();
        }

        public async Task<CarBookingDto> CreateBookingAsync(string userId, CreateCarBookingDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var booking = new ERPWebAppData.Entity.CarBooking
            {
                Id = Guid.NewGuid(),
                CustomerName = dto.CustomerName,
                CarModel = dto.CarModel,
                RegistrationNumber = dto.RegistrationNumber,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                DailyRate = dto.DailyRate,
                Status = dto.Status,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _carBookingRepo.InsertBookingAsync(booking);
            await _carBookingRepo.SaveAsync();

            int days = (dto.EndDate - dto.StartDate).Days + 1;
            decimal totalAmount = days * dto.DailyRate;

            await _dashboardService.LogActivityAsync(
                userId,
                "Car Booking Created",
                $"Customer: {dto.CustomerName}, Model: {dto.CarModel}, Total: {totalAmount}",
                "directions_car",
                "#E91E63"
            );

            return new CarBookingDto
            {
                Id = booking.Id,
                CustomerName = booking.CustomerName,
                CarModel = booking.CarModel,
                RegistrationNumber = booking.RegistrationNumber,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                DailyRate = booking.DailyRate,
                Status = booking.Status
            };
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var vehicle = new Vehicle
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Model = dto.Model,
                RegistrationNumber = dto.RegistrationNumber,
                DailyRate = dto.DailyRate,
                Status = dto.Status,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _carBookingRepo.InsertVehicleAsync(vehicle);
            await _carBookingRepo.SaveAsync();

            return new VehicleDto
            {
                Id = vehicle.Id,
                Model = vehicle.Model,
                RegistrationNumber = vehicle.RegistrationNumber,
                DailyRate = vehicle.DailyRate,
                Status = vehicle.Status
            };
        }
    }
}
