
using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class Location
    {
        [Key]
        public int LocationIndex { get; set; }
        [Required]
        public string LocationCode { get; set; } = string.Empty;
        [Required]
        public string LocationName { get; set; }= string.Empty;
        [Required]
        public string Code { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
