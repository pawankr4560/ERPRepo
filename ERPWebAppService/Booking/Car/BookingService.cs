using ERPWebAppModels.Booking;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace ERPWebAppService.Booking.Car;

public class BookingService : IBookingService
{
    private static readonly string[] ValidStatuses =
        ["Pending", "Confirmed", "Completed", "Cancelled"];

    private static readonly string[] ValidPaymentStatuses =
        ["Pending", "Paid", "Refunded", "Failed"];

    private readonly WebAppDbContext _context;

    public BookingService(WebAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookingDto>> GetAllAsync()
    {
        return await BookingQuery()
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public Task<BookingDto?> GetByIdAsync(int id)
    {
        return BookingQuery().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
    {
        ValidateDates(dto.PickupDate, dto.ReturnDate, true);
        await ValidateUserAsync(dto.UserId);

        var car = await GetCarAsync(dto.CarId);
        if (car.Status is "Maintenance" or "Inactive")
        {
            throw new InvalidOperationException("Car is not available for booking.");
        }

        await EnsureCarIsAvailableAsync(dto.CarId, dto.PickupDate, dto.ReturnDate);

        var totalDays = CalculateTotalDays(dto.PickupDate, dto.ReturnDate);
        var booking = new ERPWebAppData.Entity.Booking
        {
            BookingNumber = await GenerateBookingNumberAsync(),
            UserId = dto.UserId.Trim(),
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
        car.Status = "Booked";
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(booking.Id))!;
    }

    public async Task<BookingDto?> UpdateBookingAsync(int id, UpdateBookingDto dto)
    {
        ValidateDates(dto.PickupDate, dto.ReturnDate, false);
        ValidateStatus(dto.Status, dto.PaymentStatus);
        await ValidateUserAsync(dto.UserId);

        var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id);
        if (booking is null)
        {
            return null;
        }

        var oldCarId = booking.CarId;
        var car = await GetCarAsync(dto.CarId);
        if (car.Status is "Maintenance" or "Inactive")
        {
            throw new InvalidOperationException("Car is not available for booking.");
        }

        if (!dto.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureCarIsAvailableAsync(
                dto.CarId,
                dto.PickupDate,
                dto.ReturnDate,
                booking.Id);
        }

        var totalDays = CalculateTotalDays(dto.PickupDate, dto.ReturnDate);
        booking.UserId = dto.UserId.Trim();
        booking.CarId = dto.CarId;
        booking.PickupDate = dto.PickupDate;
        booking.ReturnDate = dto.ReturnDate;
        booking.TotalDays = totalDays;
        booking.Amount = totalDays * car.PricePerDay;
        booking.Status = Normalize(dto.Status);
        booking.PaymentStatus = Normalize(dto.PaymentStatus);

        await _context.SaveChangesAsync();
        await RefreshCarStatusAsync(oldCarId);
        if (dto.CarId != oldCarId)
        {
            await RefreshCarStatusAsync(dto.CarId);
        }

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteBookingAsync(int id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id);
        if (booking is null)
        {
            return false;
        }

        var carId = booking.CarId;
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        await RefreshCarStatusAsync(carId);
        return true;
    }

    private IQueryable<BookingDto> BookingQuery()
    {
        return _context.Bookings
            .AsNoTracking()
            .Select(x => new BookingDto
            {
                Id = x.Id,
                BookingNumber = x.BookingNumber,
                UserId = x.UserId,
                UserName = x.User == null
                    ? ""
                    : (x.User.FirstName + " " + x.User.LastName).Trim(),
                CarId = x.CarId,
                CarName = x.Car == null ? "" : $"{x.Car.Brand} {x.Car.Model}",
                PickupDate = x.PickupDate,
                ReturnDate = x.ReturnDate,
                TotalDays = x.TotalDays,
                Amount = x.Amount,
                Status = x.Status,
                PaymentStatus = x.PaymentStatus,
                CreatedDate = x.CreatedDate
            });
    }

    private async Task<ERPWebAppData.Entity.Car> GetCarAsync(int carId)
    {
        var car = await _context.Cars.FirstOrDefaultAsync(x => x.Id == carId);
        return car ?? throw new InvalidOperationException("Car not found.");
    }

    private async Task ValidateUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("User is required.");
        }

        if (!await _context.Users.AnyAsync(x => x.Id == userId))
        {
            throw new InvalidOperationException("User not found.");
        }
    }

    private async Task EnsureCarIsAvailableAsync(
        int carId,
        DateTime pickupDate,
        DateTime returnDate,
        int? bookingId = null)
    {
        var hasOverlap = await _context.Bookings.AnyAsync(x =>
            x.CarId == carId
            && (!bookingId.HasValue || x.Id != bookingId.Value)
            && x.Status != "Cancelled"
            && pickupDate < x.ReturnDate
            && returnDate > x.PickupDate);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "The selected car is already booked for these dates.");
        }
    }

    private async Task RefreshCarStatusAsync(int carId)
    {
        var car = await _context.Cars.FirstOrDefaultAsync(x => x.Id == carId);
        if (car is null || car.Status is "Maintenance" or "Inactive")
        {
            return;
        }

        var hasActiveBooking = await _context.Bookings.AnyAsync(x =>
            x.CarId == carId
            && x.Status != "Cancelled"
            && x.Status != "Completed");

        car.Status = hasActiveBooking ? "Booked" : "Available";
        await _context.SaveChangesAsync();
    }

    private async Task<string> GenerateBookingNumberAsync()
    {
        string bookingNumber;
        do
        {
            bookingNumber = $"BK-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }
        while (await _context.Bookings.AnyAsync(x => x.BookingNumber == bookingNumber));

        return bookingNumber;
    }

    private static void ValidateDates(
        DateTime pickupDate,
        DateTime returnDate,
        bool requireFuturePickup)
    {
        if (requireFuturePickup && pickupDate.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Pickup date cannot be in the past.");
        }

        if (returnDate <= pickupDate)
        {
            throw new InvalidOperationException(
                "Return date must be later than pickup date.");
        }

        if ((returnDate - pickupDate).TotalDays > 365)
        {
            throw new InvalidOperationException(
                "A booking cannot exceed 365 days.");
        }
    }

    private static int CalculateTotalDays(DateTime pickupDate, DateTime returnDate)
    {
        return Math.Max(1, (int)Math.Ceiling((returnDate - pickupDate).TotalDays));
    }

    private static void ValidateStatus(string status, string paymentStatus)
    {
        if (!ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid booking status.");
        }

        if (!ValidPaymentStatuses.Contains(
            paymentStatus,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid payment status.");
        }
    }

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return char.ToUpperInvariant(normalized[0]) + normalized[1..];
    }
}
