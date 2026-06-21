namespace ERPWebAppModels.Booking;

public class BookingOptionsDto
{
    public List<BookingOptionCarDto> Cars { get; set; } = [];
    public List<BookingOptionUserDto> Users { get; set; } = [];
}

public class BookingOptionCarDto
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BookingOptionUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
}
