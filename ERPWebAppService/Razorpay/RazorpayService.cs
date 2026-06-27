using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Payment;
using WebApp.Model.Transaction;

namespace WebApp.Service.Razorpay;

public class RazorpayService : IRazorpayService
{
    private const string ActiveStatus = "Active";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly WebAppDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RazorpayService(
        WebAppDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public RazorpayConfigResponse GetConfig()
    {
        return new RazorpayConfigResponse
        {
            KeyId = _configuration["Razorpay:KeyId"] ?? string.Empty
        };
    }

    public async Task<List<UserLoanSummaryDto>> GetMyLoansAsync(string userId, bool isAdmin)
    {
        var query = _dbContext.Loan.AsNoTracking()
            .Where(x => !x.IsDeleted && x.Active && x.Status == ActiveStatus);

        if (!isAdmin)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var loans = await query
            .OrderByDescending(x => x.F_Created_Date_Time)
            .Select(x => new
            {
                x.Id,
                x.LoanNumber,
                x.LoanAmount,
                x.EMI,
                x.Tenure
            })
            .ToListAsync();

        var loanIds = loans.Select(x => x.Id).ToList();
        var schedules = await _dbContext.LoanEMISchedule.AsNoTracking()
            .Where(x => loanIds.Contains(x.LoanId) && !x.IsDeleted && !x.IsPaid)
            .OrderBy(x => x.DueDate)
            .Select(x => new { x.LoanId, x.DueDate, x.EMIAmount })
            .ToListAsync();

        return loans.Select(loan =>
        {
            var unpaid = schedules.Where(x => x.LoanId == loan.Id).ToList();
            var next = unpaid.FirstOrDefault();

            return new UserLoanSummaryDto
            {
                Id = loan.Id,
                LoanNumber = loan.LoanNumber,
                LoanAmount = loan.LoanAmount,
                Emi = loan.EMI,
                Tenure = loan.Tenure,
                Status = ActiveStatus,
                UnpaidInstallments = unpaid.Count,
                NextDueDate = next?.DueDate,
                NextEmiAmount = next?.EMIAmount
            };
        }).ToList();
    }

    public async Task<List<LoanInstallmentDto>> GetUnpaidInstallmentsAsync(int loanId, string userId, bool isAdmin)
    {
        var loan = await GetAccessibleLoanAsync(loanId, userId, isAdmin);

        return await _dbContext.LoanEMISchedule.AsNoTracking()
            .Where(x =>
                x.LoanId == loan.Id &&
                !x.IsDeleted &&
                !x.IsPaid)
            .OrderBy(x => x.InstallmentNo)
            .Select(x => new LoanInstallmentDto
            {
                LoanId = loan.Id,
                ScheduleId = x.Id,
                InstallmentNo = x.InstallmentNo,
                DueDate = x.DueDate,
                EMIAmount = x.EMIAmount,
                PrincipalAmount = x.PrincipalAmount,
                InterestAmount = x.InterestAmount,
                OutstandingBalance = x.OutstandingBalance
            })
            .ToListAsync();
    }

    public async Task<RazorpayEmiOrderResponse> CreateEmiOrderAsync(
        RazorpayEmiOrderRequest request,
        string userId,
        bool isAdmin)
    {
        var keyId = _configuration["Razorpay:KeyId"];
        var keySecret = _configuration["Razorpay:KeySecret"];

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            throw new InvalidOperationException("Razorpay is not configured. Add KeyId and KeySecret in appsettings.");
        }

        var loan = await GetAccessibleLoanAsync(request.LoanId, userId, isAdmin);
        var schedule = await _dbContext.LoanEMISchedule.FirstOrDefaultAsync(x =>
            x.Id == request.ScheduleId &&
            x.LoanId == loan.Id &&
            !x.IsDeleted);

        if (schedule == null)
        {
            throw new InvalidOperationException("The selected installment does not belong to this loan.");
        }

        if (schedule.IsPaid)
        {
            throw new InvalidOperationException("This installment is already paid.");
        }

        EnsurePaymentAllowed(schedule.DueDate);

        var amountPaise = ToPaise(schedule.EMIAmount);
        var receipt = $"emi_{loan.Id}_{schedule.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var payload = new
        {
            amount = amountPaise,
            currency = "INR",
            receipt,
            notes = new Dictionary<string, string>
            {
                ["loanId"] = loan.Id.ToString(),
                ["scheduleId"] = schedule.Id.ToString(),
                ["loanNumber"] = loan.LoanNumber,
                ["installmentNo"] = schedule.InstallmentNo.ToString(),
                ["userId"] = loan.UserId
            }
        };

        var client = CreateRazorpayClient(keyId, keySecret);
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("orders", content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Unable to create Razorpay order: {body}");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var orderId = root.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Razorpay order id missing in response.");

        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == loan.UserId);

        return new RazorpayEmiOrderResponse
        {
            KeyId = keyId,
            OrderId = orderId,
            Amount = schedule.EMIAmount,
            AmountPaise = amountPaise,
            Currency = "INR",
            LoanNumber = loan.LoanNumber,
            InstallmentNo = schedule.InstallmentNo,
            CustomerName = $"{user?.FirstName} {user?.LastName}".Trim(),
            CustomerEmail = user?.Email ?? string.Empty,
            CustomerPhone = user != null && user.Phone > 0 ? user.Phone.ToString() : string.Empty
        };
    }

    public async Task<RazorpayEmiVerifyResponse> VerifyEmiPaymentAsync(
        RazorpayEmiVerifyRequest request,
        string userId,
        bool isAdmin)
    {
        var keySecret = _configuration["Razorpay:KeySecret"];
        if (string.IsNullOrWhiteSpace(keySecret))
        {
            throw new InvalidOperationException("Razorpay is not configured.");
        }

        if (string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||
            string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
            string.IsNullOrWhiteSpace(request.RazorpaySignature))
        {
            throw new InvalidOperationException("Payment verification details are incomplete.");
        }

        var expectedSignature = ComputeSignature(
            $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}",
            keySecret);

        if (!string.Equals(expectedSignature, request.RazorpaySignature, StringComparison.OrdinalIgnoreCase))
        {
            return new RazorpayEmiVerifyResponse
            {
                Success = false,
                Message = "Payment verification failed. Invalid signature."
            };
        }

        var loan = await GetAccessibleLoanAsync(request.LoanId, userId, isAdmin);
        var schedule = await _dbContext.LoanEMISchedule.FirstOrDefaultAsync(x =>
            x.Id == request.ScheduleId &&
            x.LoanId == loan.Id &&
            !x.IsDeleted);

        if (schedule == null)
        {
            throw new InvalidOperationException("The selected installment does not belong to this loan.");
        }

        if (schedule.IsPaid)
        {
            var existingPayment = await _dbContext.LoanPayment.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.ScheduleId == schedule.Id &&
                    x.TransactionId == request.RazorpayPaymentId);

            return new RazorpayEmiVerifyResponse
            {
                Success = true,
                Message = "This installment was already paid.",
                Payment = existingPayment == null
                    ? null
                    : MapPayment(existingPayment)
            };
        }

        EnsurePaymentAllowed(schedule.DueDate);

        var duplicateTransaction = await _dbContext.LoanPayment.AnyAsync(x =>
            !x.IsDeleted &&
            x.TransactionId == request.RazorpayPaymentId);

        if (duplicateTransaction)
        {
            throw new InvalidOperationException("This payment has already been recorded.");
        }

        var payment = new LoanPayment
        {
            LoanId = loan.Id,
            ScheduleId = schedule.Id,
            AmountPaid = schedule.EMIAmount,
            PaymentDate = DateTime.UtcNow,
            TransactionId = request.RazorpayPaymentId,
            PaymentMode = "Razorpay",
            PaymentStatus = "Success",
            Remarks = $"Razorpay order {request.RazorpayOrderId}",
            Active = true,
            IsDeleted = false,
            F_Created_Date_Time = DateTime.UtcNow
        };

        schedule.IsPaid = true;
        schedule.PaidDate = payment.PaymentDate;
        schedule.F_Updated_Date_Time = DateTime.UtcNow;

        await _dbContext.LoanPayment.AddAsync(payment);
        await _dbContext.SaveChangesAsync();

        return new RazorpayEmiVerifyResponse
        {
            Success = true,
            Message = "EMI payment completed successfully.",
            Payment = MapPayment(payment)
        };
    }

    public async Task<RazorpayBookingOrderResponse> CreateBookingOrderAsync(
        RazorpayBookingOrderRequest request,
        string userId,
        bool isAdmin)
    {
        var keyId = _configuration["Razorpay:KeyId"];
        var keySecret = _configuration["Razorpay:KeySecret"];

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
        {
            throw new InvalidOperationException("Razorpay is not configured. Add KeyId and KeySecret in appsettings.");
        }

        var booking = await GetAccessibleBookingAsync(request.BookingId, userId, isAdmin);
        var outstandingAmount = await GetBookingOutstandingAmountAsync(booking.Id, booking.Amount);
        if (outstandingAmount <= 0)
        {
            throw new InvalidOperationException("This booking is already paid.");
        }

        var serviceFeeAmount = await GetBookingServiceFeeAmountAsync(outstandingAmount);
        var totalAmount = outstandingAmount + serviceFeeAmount;
        var amountPaise = ToPaise(totalAmount);
        var receipt = $"booking_{booking.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var payload = new
        {
            amount = amountPaise,
            currency = "INR",
            receipt,
            notes = new Dictionary<string, string>
            {
                ["bookingId"] = booking.Id.ToString(),
                ["bookingNumber"] = booking.BookingNumber,
                ["bookingAmount"] = outstandingAmount.ToString("0.00"),
                ["serviceFee"] = serviceFeeAmount.ToString("0.00"),
                ["userId"] = booking.UserId
            }
        };

        var client = CreateRazorpayClient(keyId, keySecret);
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("orders", content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Unable to create Razorpay booking order: {body}");
        }

        using var document = JsonDocument.Parse(body);
        var orderId = document.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Razorpay order id missing in response.");

        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == booking.UserId);

        return new RazorpayBookingOrderResponse
        {
            KeyId = keyId,
            OrderId = orderId,
            Amount = totalAmount,
            BookingAmount = outstandingAmount,
            ServiceFeeAmount = serviceFeeAmount,
            TotalAmount = totalAmount,
            AmountPaise = amountPaise,
            Currency = "INR",
            BookingNumber = booking.BookingNumber,
            CarName = booking.Car == null ? string.Empty : $"{booking.Car.Brand} {booking.Car.Model}",
            CustomerName = $"{user?.FirstName} {user?.LastName}".Trim(),
            CustomerEmail = user?.Email ?? string.Empty,
            CustomerPhone = user != null && user.Phone > 0 ? user.Phone.ToString() : string.Empty
        };
    }

    public async Task<RazorpayBookingVerifyResponse> VerifyBookingPaymentAsync(
        RazorpayBookingVerifyRequest request,
        string userId,
        bool isAdmin)
    {
        var keySecret = _configuration["Razorpay:KeySecret"];
        if (string.IsNullOrWhiteSpace(keySecret))
        {
            throw new InvalidOperationException("Razorpay is not configured.");
        }

        if (string.IsNullOrWhiteSpace(request.RazorpayOrderId) ||
            string.IsNullOrWhiteSpace(request.RazorpayPaymentId) ||
            string.IsNullOrWhiteSpace(request.RazorpaySignature))
        {
            throw new InvalidOperationException("Payment verification details are incomplete.");
        }

        var expectedSignature = ComputeSignature(
            $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}",
            keySecret);

        if (!string.Equals(expectedSignature, request.RazorpaySignature, StringComparison.OrdinalIgnoreCase))
        {
            return new RazorpayBookingVerifyResponse
            {
                Success = false,
                Message = "Payment verification failed. Invalid signature."
            };
        }

        var booking = await GetAccessibleBookingAsync(request.BookingId, userId, isAdmin);

        var existingPayment = await _dbContext.BookingPayments.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.BookingId == booking.Id &&
                x.TransactionReference == request.RazorpayPaymentId);

        if (existingPayment != null)
        {
            return new RazorpayBookingVerifyResponse
            {
                Success = true,
                Message = "This booking payment was already recorded.",
                Payment = MapBookingPayment(existingPayment, booking)
            };
        }

        var duplicateTransaction = await _dbContext.BookingPayments.AnyAsync(x =>
            x.TransactionReference == request.RazorpayPaymentId);

        if (duplicateTransaction)
        {
            throw new InvalidOperationException("This payment has already been recorded.");
        }

        var outstandingAmount = await GetBookingOutstandingAmountAsync(booking.Id, booking.Amount);
        if (outstandingAmount <= 0)
        {
            throw new InvalidOperationException("This booking is already paid.");
        }

        var serviceFeeAmount = await GetBookingServiceFeeAmountAsync(outstandingAmount);
        var totalAmount = outstandingAmount + serviceFeeAmount;

        var payment = new ERPWebAppData.Entity.BookingPayment
        {
            BookingId = booking.Id,
            Amount = outstandingAmount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Razorpay",
            Status = "Paid",
            TransactionReference = request.RazorpayPaymentId,
            Notes = $"Razorpay order {request.RazorpayOrderId}; service fee INR {serviceFeeAmount:0.00}; charged INR {totalAmount:0.00}",
            CreatedDate = DateTime.UtcNow
        };

        _dbContext.BookingPayments.Add(payment);
        await _dbContext.SaveChangesAsync();
        await RefreshBookingPaymentStatusAsync(booking.Id);

        return new RazorpayBookingVerifyResponse
        {
            Success = true,
            Message = "Booking payment completed successfully.",
            Payment = MapBookingPayment(payment, booking)
        };
    }

    private async Task<Loan> GetAccessibleLoanAsync(int loanId, string userId, bool isAdmin)
    {
        var loan = await _dbContext.Loan.FirstOrDefaultAsync(x =>
            x.Id == loanId &&
            !x.IsDeleted);

        if (loan == null)
        {
            throw new InvalidOperationException("Loan does not exist.");
        }

        if (!loan.Active || loan.Status != ActiveStatus)
        {
            throw new InvalidOperationException("Payments can only be made for approved active loans.");
        }

        if (!isAdmin && !string.Equals(loan.UserId, userId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("You are not allowed to pay for this loan.");
        }

        return loan;
    }

    private async Task<ERPWebAppData.Entity.Booking> GetAccessibleBookingAsync(
        int bookingId,
        string userId,
        bool isAdmin)
    {
        var booking = await _dbContext.Bookings
            .Include(x => x.Car)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == bookingId);

        if (booking == null)
        {
            throw new InvalidOperationException("Booking does not exist.");
        }

        if (booking.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Payments cannot be made for a cancelled booking.");
        }

        if (!isAdmin && !string.Equals(booking.UserId, userId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("You are not allowed to pay for this booking.");
        }

        return booking;
    }

    private async Task<decimal> GetBookingOutstandingAmountAsync(int bookingId, decimal bookingAmount)
    {
        var paidAmount = await _dbContext.BookingPayments
            .Where(x => x.BookingId == bookingId && x.Status == "Paid")
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        return Math.Max(0, bookingAmount - paidAmount);
    }

    private async Task<decimal> GetBookingServiceFeeAmountAsync(decimal bookingAmount)
    {
        var setting = await _dbContext.LoanSetting.AsNoTracking().FirstOrDefaultAsync();
        var fixedCharge = Math.Max(0, setting?.BookingPaymentFixedCharge ?? 0);
        var percentageCharge = Math.Clamp(setting?.BookingPaymentPercentageCharge ?? 0, 0, 100);
        var percentageAmount = bookingAmount * percentageCharge / 100;
        return Math.Round(fixedCharge + percentageAmount, 2, MidpointRounding.AwayFromZero);
    }

    private async Task RefreshBookingPaymentStatusAsync(int bookingId)
    {
        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId);
        if (booking == null)
        {
            return;
        }

        var payments = await _dbContext.BookingPayments
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

        await _dbContext.SaveChangesAsync();
    }

    private static void EnsurePaymentAllowed(DateTime dueDate)
    {
        var today = DateTime.UtcNow.Date;
        if (today < dueDate.Date)
        {
            throw new InvalidOperationException("Payment cannot be made before the installment due date.");
        }
    }

    private static long ToPaise(decimal amount)
    {
        return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
    }

    private HttpClient CreateRazorpayClient(string keyId, string keySecret)
    {
        var client = _httpClientFactory.CreateClient("RazorpayClient");
        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        return client;
    }

    private static string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static LoanPaymentDto MapPayment(LoanPayment payment)
    {
        return new LoanPaymentDto
        {
            Id = payment.Id,
            LoanId = payment.LoanId,
            ScheduleId = payment.ScheduleId,
            AmountPaid = payment.AmountPaid,
            PaymentDate = payment.PaymentDate,
            TransactionId = payment.TransactionId,
            PaymentMode = payment.PaymentMode,
            PaymentStatus = payment.PaymentStatus,
            Remarks = payment.Remarks
        };
    }

    private static ERPWebAppModels.Booking.BookingPaymentDto MapBookingPayment(
        ERPWebAppData.Entity.BookingPayment payment,
        ERPWebAppData.Entity.Booking booking)
    {
        return new ERPWebAppModels.Booking.BookingPaymentDto
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            BookingNumber = booking.BookingNumber,
            CustomerName = booking.User == null
                ? string.Empty
                : $"{booking.User.FirstName} {booking.User.LastName}".Trim(),
            BookingAmount = booking.Amount,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            TransactionReference = payment.TransactionReference,
            Notes = payment.Notes,
            CreatedDate = payment.CreatedDate
        };
    }
}
