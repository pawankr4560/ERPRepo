using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class PlotListing : BaseEntity
    {
        public string PlotNumber { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal AreaSqFt { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Facing { get; set; } = string.Empty;
    }
}
