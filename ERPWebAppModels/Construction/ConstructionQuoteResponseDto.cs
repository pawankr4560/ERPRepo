namespace ERPWebAppModels.Construction
{
    public class ConstructionQuoteResponseDto
    {
        public int QuoteId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProductId { get; set; } 
        public int CategoryId { get; set; } 
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public int UnitId { get; set; } 
        public decimal EstimatedAmount { get; set; } 
        public decimal FinalQuotedAmount { get; set; } 
        public string DeliveryLocation { get; set; } = string.Empty;
        public DateTime RequiredDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
