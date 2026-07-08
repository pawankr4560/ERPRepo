namespace ERPWebAppModels.Construction
{
    public class ConstructionQuoteResponseDto
    {
        public int QuoteId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string DeliveryLocation { get; set; } = string.Empty;
        public DateTime RequiredDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
