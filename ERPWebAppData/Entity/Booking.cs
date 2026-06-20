using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string BookingNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CarId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public int TotalDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Car? Car { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User? User { get; set; }
    }
}
