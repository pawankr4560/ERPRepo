namespace ERPWebAppModels.Construction
{
    public class ConstructionQuoteDto
    {
        public int QuoteId { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public int UnitId { get; set; }

        public string UnitName { get; set; } = string.Empty;

        public decimal EstimatedAmount { get; set; }

        public decimal FinalQuotedAmount { get; set; }

        public string DeliveryLocation { get; set; } = string.Empty;

        public DateTime RequiredDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
