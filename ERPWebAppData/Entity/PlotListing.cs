using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class Plot
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AreaSqFt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PropertyType { get; set; } = string.Empty;

        [StringLength(50)]
        public string RoadWidth { get; set; } = string.Empty;

        [StringLength(100)]
        public string Electricity { get; set; } = string.Empty;

        [StringLength(150)]
        public string Water { get; set; } = string.Empty;

        [StringLength(100)]
        public string Registration { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public Guid SellerId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? ThumbnailUrl { get; set; }

        public Seller? Seller { get; set; }

        public ICollection<PlotImage> Images { get; set; }
            = new List<PlotImage>();

        public ICollection<PlotAmenity> PlotAmenities { get; set; }
            = new List<PlotAmenity>();

        public ICollection<SavedPlot> SavedPlots { get; set; }
            = new List<SavedPlot>();
    }
}
