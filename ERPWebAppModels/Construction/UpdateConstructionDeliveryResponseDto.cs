namespace ERPWebAppModels.Construction
{
    public class UpdateConstructionDeliveryResponseDto
    {
        public int DeliveryId { get; set; }

        public int OrderId { get; set; }

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? VehicleNumber { get; set; }

        public string? DriverName { get; set; }

        public DateTime? EstimatedArrivalTime { get; set; }

        public decimal Progress { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
