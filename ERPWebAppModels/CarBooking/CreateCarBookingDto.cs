using System;

namespace ERPWebAppModels.CarBooking
{
    public class CreateCarBookingDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DailyRate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
