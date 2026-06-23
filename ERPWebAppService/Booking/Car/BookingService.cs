using ERPWebAppModels.Booking;
using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using BookingEntity = ERPWebAppData.Entity.Booking;
using CarEntity = ERPWebAppData.Entity.Car;

namespace ERPWebAppService.Booking.Car;

public class BookingService : IBookingService
{
    private static readonly string[] ValidStatuses = ["Pending", "Confirmed", "Completed", "Cancelled"];
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;

    public BookingService(MongoDbContext context, IMongoSequenceService sequences)
    {
        _context = context;
        _sequences = sequences;
    }

    public async Task<IEnumerable<BookingDto>> GetAllAsync() =>
        (await BuildDtosAsync(await _context.Bookings.Find(Builders<BookingEntity>.Filter.Empty)
            .SortByDescending(x => x.CreatedDate).ToListAsync()));

    public async Task<BookingDto?> GetByIdAsync(int id) =>
        (await BuildDtosAsync(await _context.Bookings.Find(x => x.Id == id).ToListAsync()))
        .FirstOrDefault();

    public async Task<BookingOptionsDto> GetOptionsAsync()
    {
        var cars = await _context.Cars.Find(x =>
            x.Status != "Maintenance" && x.Status != "Inactive").ToListAsync();
        var users = await _context.Users.Find(x => x.IsActive && !x.IsDeleted).ToListAsync();
        return new BookingOptionsDto
        {
            Cars = cars.OrderBy(x => x.Brand).ThenBy(x => x.Model).Select(x => new BookingOptionCarDto
            {
                Id = x.Id, Brand = x.Brand, Model = x.Model,
                PricePerDay = x.PricePerDay, Status = x.Status
            }).ToList(),
            Users = users.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).Select(x => new BookingOptionUserDto
            {
                Id = x.Id, FirstName = x.FirstName, LastName = x.LastName, Email = x.Email
            }).ToList()
        };
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
    {
        ValidateDates(dto.PickupDate, dto.ReturnDate, true);
        await ValidateUserAsync(dto.UserId);
        var car = await GetCarAsync(dto.CarId);
        if (car.Status is "Maintenance" or "Inactive")
            throw new InvalidOperationException("Car is not available for booking.");
        await EnsureCarIsAvailableAsync(dto.CarId, dto.PickupDate, dto.ReturnDate);
        var days = CalculateTotalDays(dto.PickupDate, dto.ReturnDate);
        var booking = new BookingEntity
        {
            Id = await _sequences.GetNextAsync("Bookings"),
            BookingNumber = await GenerateBookingNumberAsync(),
            UserId = dto.UserId.Trim(), CarId = dto.CarId,
            PickupDate = dto.PickupDate, ReturnDate = dto.ReturnDate,
            TotalDays = days, Amount = days * car.PricePerDay,
            Status = "Pending", PaymentStatus = "Pending", CreatedDate = DateTime.UtcNow
        };
        await _context.Bookings.InsertOneAsync(booking);
        await _context.Cars.UpdateOneAsync(x => x.Id == car.Id,
            Builders<CarEntity>.Update.Set(x => x.Status, "Booked"));
        return (await GetByIdAsync(booking.Id))!;
    }

    public async Task<BookingDto?> UpdateBookingAsync(int id, UpdateBookingDto dto)
    {
        ValidateDates(dto.PickupDate, dto.ReturnDate, false);
        ValidateStatus(dto.Status);
        await ValidateUserAsync(dto.UserId);
        var booking = await _context.Bookings.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (booking == null) return null;
        var oldCarId = booking.CarId;
        var car = await GetCarAsync(dto.CarId);
        if (car.Status is "Maintenance" or "Inactive")
            throw new InvalidOperationException("Car is not available for booking.");
        if (!dto.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            await EnsureCarIsAvailableAsync(dto.CarId, dto.PickupDate, dto.ReturnDate, id);

        var days = CalculateTotalDays(dto.PickupDate, dto.ReturnDate);
        booking.UserId = dto.UserId.Trim();
        booking.CarId = dto.CarId;
        booking.PickupDate = dto.PickupDate;
        booking.ReturnDate = dto.ReturnDate;
        booking.TotalDays = days;
        booking.Amount = days * car.PricePerDay;
        booking.Status = Normalize(dto.Status);
        await RefreshPaymentStatusAsync(booking);
        await _context.Bookings.ReplaceOneAsync(x => x.Id == id, booking);
        await RefreshCarStatusAsync(oldCarId);
        if (oldCarId != dto.CarId) await RefreshCarStatusAsync(dto.CarId);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteBookingAsync(int id)
    {
        var booking = await _context.Bookings.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (booking == null) return false;
        await _context.Bookings.DeleteOneAsync(x => x.Id == id);
        await RefreshCarStatusAsync(booking.CarId);
        return true;
    }

    private async Task<List<BookingDto>> BuildDtosAsync(List<BookingEntity> bookings)
    {
        var users = await _context.Users.Find(Builders<User>.Filter.In(
            x => x.Id, bookings.Select(x => x.UserId).Distinct())).ToListAsync();
        var cars = await _context.Cars.Find(Builders<CarEntity>.Filter.In(
            x => x.Id, bookings.Select(x => x.CarId).Distinct())).ToListAsync();
        var userById = users.ToDictionary(x => x.Id);
        var carById = cars.ToDictionary(x => x.Id);
        return bookings.Select(x =>
        {
            userById.TryGetValue(x.UserId, out var user);
            carById.TryGetValue(x.CarId, out var car);
            return new BookingDto
            {
                Id = x.Id, BookingNumber = x.BookingNumber, UserId = x.UserId,
                UserName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim(),
                CarId = x.CarId, CarName = car == null ? "" : $"{car.Brand} {car.Model}",
                PickupDate = x.PickupDate, ReturnDate = x.ReturnDate, TotalDays = x.TotalDays,
                Amount = x.Amount, Status = x.Status, PaymentStatus = x.PaymentStatus,
                CreatedDate = x.CreatedDate
            };
        }).ToList();
    }

    private async Task<CarEntity> GetCarAsync(int id) =>
        await _context.Cars.Find(x => x.Id == id).FirstOrDefaultAsync()
        ?? throw new InvalidOperationException("Car not found.");

    private async Task ValidateUserAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !await _context.Users.Find(x => x.Id == id).AnyAsync())
            throw new InvalidOperationException("User not found.");
    }

    private async Task EnsureCarIsAvailableAsync(
        int carId, DateTime pickup, DateTime returned, int? bookingId = null)
    {
        if (await _context.Bookings.Find(x =>
            x.CarId == carId && (!bookingId.HasValue || x.Id != bookingId.Value)
            && x.Status != "Cancelled" && pickup < x.ReturnDate && returned > x.PickupDate).AnyAsync())
            throw new InvalidOperationException("The selected car is already booked for these dates.");
    }

    private async Task RefreshCarStatusAsync(int carId)
    {
        var car = await _context.Cars.Find(x => x.Id == carId).FirstOrDefaultAsync();
        if (car == null || car.Status is "Maintenance" or "Inactive") return;
        var booked = await _context.Bookings.Find(x =>
            x.CarId == carId && x.Status != "Cancelled" && x.Status != "Completed").AnyAsync();
        await _context.Cars.UpdateOneAsync(x => x.Id == carId,
            Builders<CarEntity>.Update.Set(x => x.Status, booked ? "Booked" : "Available"));
    }

    private async Task RefreshPaymentStatusAsync(BookingEntity booking)
    {
        var payments = await _context.BookingPayments.Find(x => x.BookingId == booking.Id).ToListAsync();
        var paid = payments.Where(x => x.Status == "Paid").Sum(x => x.Amount);
        if (paid > booking.Amount)
            throw new InvalidOperationException("The updated booking amount cannot be less than the amount already paid.");
        booking.PaymentStatus = paid >= booking.Amount ? "Paid"
            : payments.Any(x => x.Status == "Failed") ? "Failed"
            : payments.Any(x => x.Status == "Refunded") && paid == 0 ? "Refunded" : "Pending";
    }

    private async Task<string> GenerateBookingNumberAsync()
    {
        string number;
        do number = $"BK-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        while (await _context.Bookings.Find(x => x.BookingNumber == number).AnyAsync());
        return number;
    }

    private static void ValidateDates(DateTime pickup, DateTime returned, bool future)
    {
        if (future && pickup.Date < DateTime.Today)
            throw new InvalidOperationException("Pickup date cannot be in the past.");
        if (returned <= pickup)
            throw new InvalidOperationException("Return date must be later than pickup date.");
        if ((returned - pickup).TotalDays > 365)
            throw new InvalidOperationException("A booking cannot exceed 365 days.");
    }
    private static int CalculateTotalDays(DateTime pickup, DateTime returned) =>
        Math.Max(1, (int)Math.Ceiling((returned - pickup).TotalDays));
    private static void ValidateStatus(string status)
    {
        if (!ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid booking status.");
    }
    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return char.ToUpperInvariant(normalized[0]) + normalized[1..];
    }
}
