using System;

namespace ERPWebAppModels.CarBooking
{
    public class CarBookingDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DailyRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int BookingDays => (EndDate - StartDate).Days + 1;
        public decimal TotalAmount => BookingDays * DailyRate;
    }
}
