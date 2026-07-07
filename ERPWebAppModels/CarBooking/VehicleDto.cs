using System;

namespace ERPWebAppModels.CarBooking
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
