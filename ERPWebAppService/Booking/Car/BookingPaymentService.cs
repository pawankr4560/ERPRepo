using ERPWebAppModels.Booking;
using MongoDB.Driver;
using WebApp.Data;
using BookingEntity = ERPWebAppData.Entity.Booking;
using PaymentEntity = ERPWebAppData.Entity.BookingPayment;

namespace ERPWebAppService.Booking.Car;

public class BookingPaymentService : IBookingPaymentService
{
    private static readonly string[] ValidMethods = ["Cash", "Card", "UPI", "Bank Transfer", "Cheque"];
    private static readonly string[] ValidStatuses = ["Pending", "Paid", "Failed", "Refunded"];
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;

    public BookingPaymentService(MongoDbContext context, IMongoSequenceService sequences)
    {
        _context = context;
        _sequences = sequences;
    }

    public async Task<IEnumerable<BookingPaymentDto>> GetAllAsync(int? bookingId = null)
    {
        var filter = bookingId.HasValue
            ? Builders<PaymentEntity>.Filter.Eq(x => x.BookingId, bookingId.Value)
            : Builders<PaymentEntity>.Filter.Empty;
        return await BuildDtosAsync(await _context.BookingPayments.Find(filter)
            .SortByDescending(x => x.PaymentDate).ToListAsync());
    }

    public async Task<BookingPaymentDto?> GetByIdAsync(int id) =>
        (await BuildDtosAsync(await _context.BookingPayments.Find(x => x.Id == id).ToListAsync()))
        .FirstOrDefault();

    public async Task<BookingPaymentDto> CreateAsync(BookingPaymentDto dto)
    {
        var booking = await ValidateAsync(dto);
        await EnsureAmountIsValidAsync(booking.Id, booking.Amount, dto.Amount, dto.Status);
        var payment = new PaymentEntity
        {
            Id = await _sequences.GetNextAsync("BookingPayments"),
            BookingId = dto.BookingId, Amount = dto.Amount, PaymentDate = dto.PaymentDate,
            PaymentMethod = Canonical(dto.PaymentMethod, ValidMethods),
            Status = Canonical(dto.Status, ValidStatuses),
            TransactionReference = string.IsNullOrWhiteSpace(dto.TransactionReference)
                ? GenerateReference() : dto.TransactionReference.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            CreatedDate = DateTime.UtcNow
        };
        await _context.BookingPayments.InsertOneAsync(payment);
        await RefreshBookingPaymentStatusAsync(dto.BookingId);
        return (await GetByIdAsync(payment.Id))!;
    }

    public async Task<BookingPaymentDto?> UpdateAsync(int id, BookingPaymentDto dto)
    {
        var payment = await _context.BookingPayments.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (payment == null) return null;
        var oldBookingId = payment.BookingId;
        var booking = await ValidateAsync(dto);
        await EnsureAmountIsValidAsync(booking.Id, booking.Amount, dto.Amount, dto.Status, id);
        payment.BookingId = dto.BookingId;
        payment.Amount = dto.Amount;
        payment.PaymentDate = dto.PaymentDate;
        payment.PaymentMethod = Canonical(dto.PaymentMethod, ValidMethods);
        payment.Status = Canonical(dto.Status, ValidStatuses);
        if (!string.IsNullOrWhiteSpace(dto.TransactionReference))
            payment.TransactionReference = dto.TransactionReference.Trim();
        payment.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        await _context.BookingPayments.ReplaceOneAsync(x => x.Id == id, payment);
        await RefreshBookingPaymentStatusAsync(oldBookingId);
        if (oldBookingId != dto.BookingId) await RefreshBookingPaymentStatusAsync(dto.BookingId);
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _context.BookingPayments.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (payment == null) return false;
        await _context.BookingPayments.DeleteOneAsync(x => x.Id == id);
        await RefreshBookingPaymentStatusAsync(payment.BookingId);
        return true;
    }

    private async Task<List<BookingPaymentDto>> BuildDtosAsync(List<PaymentEntity> payments)
    {
        var bookingIds = payments.Select(x => x.BookingId).Distinct();
        var bookings = await _context.Bookings.Find(
            Builders<BookingEntity>.Filter.In(x => x.Id, bookingIds)).ToListAsync();
        var users = await _context.Users.Find(
            Builders<WebApp.Data.Entity.User>.Filter.In(x => x.Id, bookings.Select(x => x.UserId).Distinct()))
            .ToListAsync();
        var bookingById = bookings.ToDictionary(x => x.Id);
        var userById = users.ToDictionary(x => x.Id);
        return payments.Select(x =>
        {
            bookingById.TryGetValue(x.BookingId, out var booking);
            WebApp.Data.Entity.User? user = null;
            if (booking != null) userById.TryGetValue(booking.UserId, out user);
            return new BookingPaymentDto
            {
                Id = x.Id, BookingId = x.BookingId,
                BookingNumber = booking?.BookingNumber ?? "",
                CustomerName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim(),
                BookingAmount = booking?.Amount ?? 0, Amount = x.Amount,
                PaymentDate = x.PaymentDate, PaymentMethod = x.PaymentMethod,
                Status = x.Status, TransactionReference = x.TransactionReference,
                Notes = x.Notes, CreatedDate = x.CreatedDate
            };
        }).ToList();
    }

    private async Task<BookingEntity> ValidateAsync(BookingPaymentDto dto)
    {
        var booking = await _context.Bookings.Find(x => x.Id == dto.BookingId).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Booking not found.");
        if (booking.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Payments cannot be recorded for a cancelled booking.");
        if (!ValidMethods.Contains(dto.PaymentMethod, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid payment method.");
        if (!ValidStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid payment status.");
        return booking;
    }

    private async Task EnsureAmountIsValidAsync(
        int bookingId, decimal bookingAmount, decimal amount, string status, int? paymentId = null)
    {
        var paid = (await _context.BookingPayments.Find(x =>
                x.BookingId == bookingId && x.Status == "Paid"
                && (!paymentId.HasValue || x.Id != paymentId.Value)).ToListAsync())
            .Sum(x => x.Amount);
        var proposed = status.Equals("Paid", StringComparison.OrdinalIgnoreCase) ? paid + amount : paid;
        if (amount > bookingAmount || proposed > bookingAmount)
            throw new InvalidOperationException(
                $"Payment exceeds the outstanding amount of {bookingAmount - paid:0.00}.");
    }

    private async Task RefreshBookingPaymentStatusAsync(int bookingId)
    {
        var booking = await _context.Bookings.Find(x => x.Id == bookingId).FirstOrDefaultAsync();
        if (booking == null) return;
        var payments = await _context.BookingPayments.Find(x => x.BookingId == bookingId).ToListAsync();
        var paid = payments.Where(x => x.Status == "Paid").Sum(x => x.Amount);
        var status = paid >= booking.Amount ? "Paid"
            : payments.Any(x => x.Status == "Failed") ? "Failed"
            : payments.Any(x => x.Status == "Refunded") && paid == 0 ? "Refunded" : "Pending";
        await _context.Bookings.UpdateOneAsync(x => x.Id == bookingId,
            Builders<BookingEntity>.Update.Set(x => x.PaymentStatus, status));
    }

    private static string Canonical(string value, string[] values) =>
        values.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    private static string GenerateReference() => $"PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
}
