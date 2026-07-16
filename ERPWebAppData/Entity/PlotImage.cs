using System.ComponentModel.DataAnnotations;

namespace ERPWebAppData.Entity
{
    public class PlotImage
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PlotId { get; set; }

        [Required]
        [StringLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsDeleted { get; set; }

        public Plot? Plot
        {
            get; set;
        }
    }
}
