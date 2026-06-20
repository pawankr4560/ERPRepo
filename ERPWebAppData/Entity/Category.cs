using System.ComponentModel.DataAnnotations;

namespace ERPWebAppData.Entity
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Car> Cars { get; set; }
            = new List<Car>();
    }
}
