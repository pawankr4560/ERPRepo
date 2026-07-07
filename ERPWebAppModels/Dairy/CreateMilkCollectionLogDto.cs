using System;

namespace ERPWebAppModels.Dairy
{
    public class CreateMilkCollectionLogDto
    {
        public string FarmerName { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public decimal QuantityInLiters { get; set; }
        public decimal FatPercentage { get; set; }
        public decimal RatePerLiter { get; set; }
        public DateTime Date { get; set; }
    }
}
