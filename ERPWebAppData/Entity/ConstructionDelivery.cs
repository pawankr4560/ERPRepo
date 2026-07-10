using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ERPWebAppData.Entity
{
    public class ConstructionDelivery
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [MaxLength(50)]
        public string VehicleNumber { get; set; } = string.Empty;

        [MaxLength(150)]
        public string DriverName { get; set; } = string.Empty;

        public int Status { get; set; }

        public DateTime? EstimatedArrivalTime { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Progress { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public ConstructionOrder? Order { get; set; }
    }
}
