using System;

namespace ERPWebAppModels.Agriculture
{
    public class AgricultureStockItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string QuantityLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
