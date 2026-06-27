namespace WebApp.Model.Payment;

public class RazorpayBookingOrderResponse
{
    public string KeyId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BookingAmount { get; set; }
    public decimal ServiceFeeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public long AmountPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public string BookingNumber { get; set; } = string.Empty;
    public string CarName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
}
