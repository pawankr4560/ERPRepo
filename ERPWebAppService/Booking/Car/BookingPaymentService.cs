using ERPWebAppModels.Booking;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace ERPWebAppService.Booking.Car;

public class BookingPaymentService : IBookingPaymentService
{
    private static readonly string[] ValidMethods =
        ["Cash", "Card", "UPI", "Bank Transfer", "Cheque"];

    private static readonly string[] ValidStatuses =
        ["Pending", "Paid", "Failed", "Refunded"];

    private readonly WebAppDbContext _context;

    public BookingPaymentService(WebAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookingPaymentDto>> GetAllAsync(int? bookingId = null)
    {
        var query = PaymentQuery();
        if (bookingId.HasValue)
        {
            query = query.Where(x => x.BookingId == bookingId.Value);
        }

        return await query.OrderByDescending(x => x.PaymentDate).ToListAsync();
    }

    public Task<BookingPaymentDto?> GetByIdAsync(int id)
    {
        return PaymentQuery().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<BookingPaymentDto> CreateAsync(BookingPaymentDto dto)
    {
        var booking = await ValidateAsync(dto);
        await EnsureAmountIsValidAsync(
            booking.Id,
            booking.Amount,
            dto.Amount,
            dto.Status);

        var payment = new ERPWebAppData.Entity.BookingPayment
        {
            BookingId = dto.BookingId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate,
            PaymentMethod = Canonical(dto.PaymentMethod, ValidMethods),
            Status = Canonical(dto.Status, ValidStatuses),
            TransactionReference = string.IsNullOrWhiteSpace(dto.TransactionReference)
                ? GenerateReference()
                : dto.TransactionReference.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        _context.BookingPayments.Add(payment);
        await _context.SaveChangesAsync();
        await RefreshBookingPaymentStatusAsync(dto.BookingId);

        return (await GetByIdAsync(payment.Id))!;
    }

    public async Task<BookingPaymentDto?> UpdateAsync(int id, BookingPaymentDto dto)
    {
        var payment = await _context.BookingPayments.FirstOrDefaultAsync(x => x.Id == id);
        if (payment is null)
        {
            return null;
        }

        var oldBookingId = payment.BookingId;
        var booking = await ValidateAsync(dto);
        await EnsureAmountIsValidAsync(
            booking.Id,
            booking.Amount,
            dto.Amount,
            dto.Status,
            id);

        payment.BookingId = dto.BookingId;
        payment.Amount = dto.Amount;
        payment.PaymentDate = dto.PaymentDate;
        payment.PaymentMethod = Canonical(dto.PaymentMethod, ValidMethods);
        payment.Status = Canonical(dto.Status, ValidStatuses);
        payment.TransactionReference = string.IsNullOrWhiteSpace(dto.TransactionReference)
            ? payment.TransactionReference
            : dto.TransactionReference.Trim();
        payment.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();

        await _context.SaveChangesAsync();
        await RefreshBookingPaymentStatusAsync(oldBookingId);
        if (oldBookingId != dto.BookingId)
        {
            await RefreshBookingPaymentStatusAsync(dto.BookingId);
        }

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _context.BookingPayments.FirstOrDefaultAsync(x => x.Id == id);
        if (payment is null)
        {
            return false;
        }

        var bookingId = payment.BookingId;
        _context.BookingPayments.Remove(payment);
        await _context.SaveChangesAsync();
        await RefreshBookingPaymentStatusAsync(bookingId);
        return true;
    }

    private IQueryable<BookingPaymentDto> PaymentQuery()
    {
        return _context.BookingPayments.AsNoTracking().Select(x => new BookingPaymentDto
        {
            Id = x.Id,
            BookingId = x.BookingId,
            BookingNumber = x.Booking == null ? "" : x.Booking.BookingNumber,
            CustomerName = x.Booking == null || x.Booking.User == null
                ? ""
                : (x.Booking.User.FirstName + " " + x.Booking.User.LastName).Trim(),
            BookingAmount = x.Booking == null ? 0 : x.Booking.Amount,
            Amount = x.Amount,
            PaymentDate = x.PaymentDate,
            PaymentMethod = x.PaymentMethod,
            Status = x.Status,
            TransactionReference = x.TransactionReference,
            Notes = x.Notes,
            CreatedDate = x.CreatedDate
        });
    }

    private async Task<ERPWebAppData.Entity.Booking> ValidateAsync(BookingPaymentDto dto)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == dto.BookingId)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Payments cannot be recorded for a cancelled booking.");
        }

        if (!ValidMethods.Contains(dto.PaymentMethod, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid payment method.");
        }

        if (!ValidStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid payment status.");
        }

        return booking;
    }

    private async Task EnsureAmountIsValidAsync(
        int bookingId,
        decimal bookingAmount,
        decimal amount,
        string status,
        int? paymentId = null)
    {
        var paidAmount = await _context.BookingPayments
            .Where(x =>
                x.BookingId == bookingId
                && x.Status == "Paid"
                && (!paymentId.HasValue || x.Id != paymentId.Value))
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var proposedPaidAmount = status.Equals("Paid", StringComparison.OrdinalIgnoreCase)
            ? paidAmount + amount
            : paidAmount;

        if (amount > bookingAmount || proposedPaidAmount > bookingAmount)
        {
            throw new InvalidOperationException(
                $"Payment exceeds the outstanding amount of {bookingAmount - paidAmount:0.00}.");
        }
    }

    private async Task RefreshBookingPaymentStatusAsync(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId);
        if (booking is null)
        {
            return;
        }

        var payments = await _context.BookingPayments
            .Where(x => x.BookingId == bookingId)
            .Select(x => new { x.Amount, x.Status })
            .ToListAsync();

        var paid = payments
            .Where(x => x.Status == "Paid")
            .Sum(x => x.Amount);

        booking.PaymentStatus = paid >= booking.Amount
            ? "Paid"
            : payments.Any(x => x.Status == "Failed")
                ? "Failed"
                : payments.Any(x => x.Status == "Refunded") && paid == 0
                    ? "Refunded"
                    : "Pending";

        await _context.SaveChangesAsync();
    }

    private static string Canonical(string value, string[] validValues)
    {
        return validValues.First(x =>
            x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string GenerateReference()
    {
        return $"PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }
}
