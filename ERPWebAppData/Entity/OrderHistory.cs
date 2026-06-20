using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class OrderHistory : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public float Price { get; set; }

        [Required]
        public string Image { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Address { get; set; } = string.Empty;
    }
}
