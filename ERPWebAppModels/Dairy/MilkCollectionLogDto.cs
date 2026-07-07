using System;

namespace ERPWebAppModels.Dairy
{
    public class MilkCollectionLogDto
    {
        public Guid Id { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public decimal QuantityInLiters { get; set; }
        public decimal FatPercentage { get; set; }
        public decimal RatePerLiter { get; set; }
        public decimal TotalAmount => QuantityInLiters * RatePerLiter;
        public DateTime Date { get; set; }
    }
}
