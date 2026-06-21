using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebAppData.Entity;

public class BookingPayment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BookingId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    [Required]
    [MaxLength(30)]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Paid";

    [Required]
    [MaxLength(100)]
    public string TransactionReference { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public virtual Booking? Booking { get; set; }
}
