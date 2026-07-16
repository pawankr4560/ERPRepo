using System.ComponentModel.DataAnnotations;

namespace ERPWebAppData.Entity
{
    public class PlotAmenity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PlotId { get; set; }

        public int AmenityId { get; set; }

        public Plot? Plot { get; set; }

        public Amenity? Amenity { get; set; }
    }
}
