namespace ERPWebAppModels.Construction
{
    public class ConstructionOrderDto
    {
        public int OrderId { get; set; }

        public int QuoteId { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public int UnitId { get; set; }

        public string UnitName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string DeliveryLocation { get; set; } = string.Empty;

        public DateTime DeliveryDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
