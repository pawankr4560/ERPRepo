namespace ERPWebAppModels.Plot
{
    public class PlotDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public decimal AreaSqFt { get; set; }

        public decimal Price { get; set; }

        public string Status { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public string RoadWidth { get; set; } = string.Empty;

        public string Electricity { get; set; } = string.Empty;

        public string Water { get; set; } = string.Empty;

        public string Registration { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public SellerDto Seller { get; set; } = new();

        public List<PlotImageDto> Images { get; set; } = new();

        public List<string> Amenities { get; set; } = new();

        public bool IsSaved { get; set; }
    }
    public class SellerDto
    {
        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }
    public class PlotImageDto
    {
        public Guid Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
