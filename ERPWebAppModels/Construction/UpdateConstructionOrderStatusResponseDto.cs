namespace ERPWebAppModels.Construction
{
    public class UpdateConstructionOrderStatusResponseDto
    {
        public int OrderId { get; set; }

        public int Status { get; set; }

        public int? DeliveryId { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
