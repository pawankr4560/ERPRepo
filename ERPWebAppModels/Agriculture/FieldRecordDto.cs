using System;

namespace ERPWebAppModels.Agriculture
{
    public class FieldRecordDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Crop { get; set; } = string.Empty;
        public decimal AreaAcres { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastSprayedDate { get; set; }
    }
}
