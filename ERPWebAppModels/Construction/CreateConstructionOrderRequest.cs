using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Construction
{
    public class CreateConstructionOrderRequest
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }

        [Required]
        public int UnitId { get; set; }

        [Required]
        [StringLength(250)]
        public string DeliveryLocation { get; set; } = string.Empty;

        [Required]
        public DateTime RequiredDate { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        public string ContactNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
