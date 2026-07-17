using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Plot;

public class AdminPlotRequest
{
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal AreaSqFt { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string PropertyType { get; set; } = string.Empty;

    [StringLength(50)] public string RoadWidth { get; set; } = string.Empty;
    [StringLength(100)] public string Electricity { get; set; } = string.Empty;
    [StringLength(150)] public string Water { get; set; } = string.Empty;
    [StringLength(100)] public string Registration { get; set; } = string.Empty;
    [StringLength(1000)] public string Description { get; set; } = string.Empty;
    // Accept both absolute URLs and application-relative image paths (for example /images/plots/1.jpg).
    [StringLength(1000)] public string? ThumbnailUrl { get; set; }
    [Required, StringLength(150)] public string SellerName { get; set; } = string.Empty;
    [Required, StringLength(20)] public string SellerPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<string> ImageUrls { get; set; } = new();
    public List<int> AmenityIds { get; set; } = new();
}

public class AdminAmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AdminPlotVisitDto
{
    public Guid Id { get; set; }
    public Guid PlotId { get; set; }
    public string PlotTitle { get; set; } = string.Empty;
    public string PlotLocation { get; set; } = string.Empty;
    public decimal PlotAreaSqFt { get; set; }
    public decimal PlotPrice { get; set; }
    public string PlotStatus { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public DateOnly VisitDate { get; set; }
    public TimeOnly VisitTime { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdatePlotVisitStatusRequest
{
    [Required] public string Status { get; set; } = string.Empty;
}

public class AdminPlotDto : AdminPlotRequest
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
