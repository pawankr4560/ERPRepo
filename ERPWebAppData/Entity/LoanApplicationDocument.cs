using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class LoanApplicationDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string DocumentUrl { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
