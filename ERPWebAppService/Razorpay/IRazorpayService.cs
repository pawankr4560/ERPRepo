using WebApp.Model.Payment;
using WebApp.Model.Transaction;

namespace WebApp.Service.Razorpay;

public interface IRazorpayService
{
    RazorpayConfigResponse GetConfig();
    Task<List<UserLoanSummaryDto>> GetMyLoansAsync(string userId, bool isAdmin);
    Task<List<LoanInstallmentDto>> GetUnpaidInstallmentsAsync(int loanId, string userId, bool isAdmin);
    Task<RazorpayEmiOrderResponse> CreateEmiOrderAsync(RazorpayEmiOrderRequest request, string userId, bool isAdmin);
    Task<RazorpayEmiVerifyResponse> VerifyEmiPaymentAsync(RazorpayEmiVerifyRequest request, string userId, bool isAdmin);
    Task<RazorpayBookingOrderResponse> CreateBookingOrderAsync(RazorpayBookingOrderRequest request, string userId, bool isAdmin);
    Task<RazorpayBookingVerifyResponse> VerifyBookingPaymentAsync(RazorpayBookingVerifyRequest request, string userId, bool isAdmin);
}
