using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Booking;

public class BookingPaymentDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int BookingId { get; set; }

    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal BookingAmount { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    [MaxLength(30)]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Paid";

    [MaxLength(100)]
    public string TransactionReference { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; }
}
