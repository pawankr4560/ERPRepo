namespace ERPWebAppModels.Construction
{
    public class ConstructionQuoteRequest
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public decimal EstimatedAmount { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime RequiredDate { get; set; }
        public string ContactNumber { get; set; }
        public string Notes { get; set; }
    }
}
