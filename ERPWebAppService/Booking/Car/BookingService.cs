using ERPWebAppModels.Booking;
using Microsoft.EntityFrameworkCore;
using System;
using WebApp.Data;

namespace ERPWebAppService.Booking.Car
{
    public class BookingService : IBookingService
    {
        private readonly WebAppDbContext _context;

        public BookingService(WebAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookingDto>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(x => x.Car)
                .Select(x => new BookingDto
                {
                    Id = x.Id,
                    BookingNumber = x.BookingNumber,
                    UserId = x.UserId,
                    CarId = x.CarId,
                    CarName = x.Car != null
                                ? $"{x.Car.Brand} {x.Car.Model}"
                                : "",
                    PickupDate = x.PickupDate,
                    ReturnDate = x.ReturnDate,
                    TotalDays = x.TotalDays,
                    Amount = x.Amount,
                    Status = x.Status,
                    PaymentStatus = x.PaymentStatus,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
        {
            if (dto.ReturnDate <= dto.PickupDate)
                throw new Exception("Return date must be greater than pickup date.");

            var car = await _context.Cars.FirstOrDefaultAsync(x => x.Id == dto.CarId);

            if (car == null)
                throw new Exception("Car not found.");

            if (car.Status != "Available")
                throw new Exception("Car is not available for booking.");

            int totalDays = (dto.ReturnDate.Date - dto.PickupDate.Date).Days;

            var booking = new ERPWebAppData.Entity.Booking
            {
                BookingNumber = $"BK-{DateTime.Now:yyyyMMddHHmmss}",
                UserId = dto.UserId,
                CarId = dto.CarId,
                PickupDate = dto.PickupDate,
                ReturnDate = dto.ReturnDate,
                TotalDays = totalDays,
                Amount = totalDays * car.PricePerDay,
                Status = "Pending",
                PaymentStatus = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);

            // Mark car as booked
            car.Status = "Booked";

            await _context.SaveChangesAsync();

            return new BookingDto
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                UserId = booking.UserId,
                CarId = booking.CarId,
                CarName = $"{car.Brand} {car.Model}",
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                TotalDays = booking.TotalDays,
                Amount = booking.Amount,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                CreatedDate = booking.CreatedDate
            };
        }

    }
}
