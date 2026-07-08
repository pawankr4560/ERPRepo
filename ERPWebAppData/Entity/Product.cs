using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Data.Entity
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int CategoryId { get; set; }
        public int SubcategoryId { get; set; }

        public int UnitId { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public decimal Price { get; set; }
        public bool IsDeleted { get; set; }
        [Required]
        [StringLength(200)]
        public string? Description { get; set; } = string.Empty;

        [Required]
        public string? Image { get; set; } = string.Empty;
    }
}
