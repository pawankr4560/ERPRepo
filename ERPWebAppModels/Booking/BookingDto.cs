namespace ERPWebAppModels.Booking
{
    public class BookingDto
    {
        public int Id { get; set; }

        public string BookingNumber { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public int CarId { get; set; }

        public string CarName { get; set; } = string.Empty;

        public DateTime PickupDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int TotalDays { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
