namespace WebApp.Model.Payment;

public class RazorpayEmiOrderResponse
{
    public string KeyId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public long AmountPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public string LoanNumber { get; set; } = string.Empty;
    public int InstallmentNo { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
}
