namespace ERPWebAppModels.Construction
{
    public class UpdateQuotePriceResponseDto
    {
        public int QuoteId { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal EstimatedAmount { get; set; }

        public decimal FinalQuotedAmount { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
