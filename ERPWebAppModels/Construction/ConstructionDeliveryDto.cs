namespace ERPWebAppModels.Construction
{
    public class ConstructionDeliveryDto
    {
        public int DeliveryId { get; set; }

        public int OrderId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string VehicleNumber { get; set; } = string.Empty;

        public string DriverName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? Eta { get; set; }

        public decimal Progress { get; set; }
    }
}
