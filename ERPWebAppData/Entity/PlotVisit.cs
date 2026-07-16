using System.ComponentModel.DataAnnotations;
using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class PlotVisit
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PlotId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public DateOnly VisitDate { get; set; }

        [Required]
        public TimeOnly VisitTime { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Plot? Plot { get; set; }

        public User? User { get; set; }
    }
}
