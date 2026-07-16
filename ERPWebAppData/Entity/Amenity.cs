using System.ComponentModel.DataAnnotations;

namespace ERPWebAppData.Entity
{
    public class Amenity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<PlotAmenity> PlotAmenities { get; set; }
            = new List<PlotAmenity>();
    }
}
