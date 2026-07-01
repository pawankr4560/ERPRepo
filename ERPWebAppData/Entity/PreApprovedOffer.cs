using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ERPWebAppData.Entity
{
    public class PreApprovedOffer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string OfferCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal InterestRate { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
