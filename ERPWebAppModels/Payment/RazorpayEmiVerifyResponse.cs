using WebApp.Model.Transaction;

namespace WebApp.Model.Payment;

public class RazorpayEmiVerifyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoanPaymentDto? Payment { get; set; }
}
