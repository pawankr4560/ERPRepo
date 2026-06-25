namespace WebApp.Model.Payment;

public class RazorpayEmiVerifyRequest
{
    public int LoanId { get; set; }
    public int ScheduleId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}
