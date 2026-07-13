namespace ERPWebAppModels.Construction
{
    public class UpdateConstructionDeliveryRequest
    {
        public string? VehicleNumber { get; set; }

        public string? DriverName { get; set; }

        public DateTime? EstimatedArrivalTime { get; set; }

        public int Status { get; set; }
    }
}
