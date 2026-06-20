using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Categorie { get; set; } = string.Empty;

        public int StockQty { get; set; }
        public int UOMIndex { get; set; }
        public int LocationIndex { get; set; }
        public bool Status { get; set; }
        public float Price { get; set; }
        [Required]
        [StringLength(200)]
        public string? Description { get; set; } = string.Empty;

        [Required]
        public string? Image { get; set; } = string.Empty;
    }
}
