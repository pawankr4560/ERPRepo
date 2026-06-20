using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Booking;

public class CreateCarDto
{
    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Transmission { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string FuelType { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Seats { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal PricePerDay { get; set; }

    [MaxLength(500)]
    [Url]
    public string? ImageUrl { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Available";
}
