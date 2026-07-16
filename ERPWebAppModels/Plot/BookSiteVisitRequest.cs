using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Plot
{
    public class BookSiteVisitRequest
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public DateOnly VisitDate { get; set; }

        [Required]
        public TimeOnly VisitTime { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
