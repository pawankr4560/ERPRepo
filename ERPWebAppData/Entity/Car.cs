using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPWebAppData.Entity
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public int CategoryId { get; set; }

        [MaxLength(20)]
        public string Transmission { get; set; } = string.Empty;

        [MaxLength(20)]
        public string FuelType { get; set; } = string.Empty;

        public int Seats { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Available";

        // Navigation Properties
        public virtual Category? Category { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}
