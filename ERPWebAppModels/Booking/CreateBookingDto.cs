using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Booking;

public class CreateBookingDto
{
    [Range(1, int.MaxValue)]
    public int CarId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateTime PickupDate { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }
}
