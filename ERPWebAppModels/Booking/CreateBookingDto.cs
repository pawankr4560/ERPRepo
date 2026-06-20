namespace ERPWebAppModels.Booking
{
    public class CreateBookingDto
    {
        public int CarId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
    }
}
