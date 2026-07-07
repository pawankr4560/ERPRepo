using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class Vehicle : BaseEntity
    {
        public string Model { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal DailyRate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
