namespace ERPWebAppModels.Plot
{
    public class SavedPlotDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public decimal AreaSqFt { get; set; }

        public decimal Price { get; set; }

        public string Status { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = string.Empty;

        public DateTime SavedAt { get; set; }

        public bool IsSaved { get; set; }
    }
}
