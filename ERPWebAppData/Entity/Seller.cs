using System.ComponentModel.DataAnnotations;

namespace ERPWebAppData.Entity
{
    public class Seller
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Plot> Plots { get; set; }
            = new List<Plot>();
    }
}
