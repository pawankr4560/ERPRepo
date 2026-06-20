namespace ERPWebAppModels.Booking;

public class CreateCarDto
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int CategoryId { get; set; }
    public string Transmission { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public int Seats { get; set; }
    public decimal PricePerDay { get; set; }
    public string? ImageUrl { get; set; }
}
