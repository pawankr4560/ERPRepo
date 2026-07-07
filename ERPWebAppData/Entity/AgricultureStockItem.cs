using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class AgricultureStockItem : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string QuantityLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
