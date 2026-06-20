using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class UnitOfMeasure
    {
        [Key]
        public int UOMIndex { get; set; }
        [Required]
        public string UOMCode { get; set; } = string.Empty;
        [Required]
        public string UOMName { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
