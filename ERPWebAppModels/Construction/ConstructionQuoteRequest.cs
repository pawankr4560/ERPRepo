namespace ERPWebAppModels.Construction
{
    public class ConstructionQuoteRequest
    {
        public string CategoryId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string DeliveryLocation { get; set; } = string.Empty;
        public string RequiredDate { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
