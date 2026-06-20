using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Booking;

public class UpdateBookingDto : CreateBookingDto
{
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Required]
    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";
}
