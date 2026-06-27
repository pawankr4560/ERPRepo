using ERPWebAppModels.Booking;

namespace WebApp.Model.Payment;

public class RazorpayBookingVerifyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public BookingPaymentDto? Payment { get; set; }
}
