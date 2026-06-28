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

        public int Quantity { get; set; } = 1;

        public float LineTotal { get; set; }

        [Required]
        public string Image { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string OrderType { get; set; } = "Delivery";

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = "Razorpay";

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Paid";

        [StringLength(100)]
        public string TransactionReference { get; set; } = string.Empty;

        [StringLength(100)]
        public string RazorpayOrderId { get; set; } = string.Empty;

        public float ServiceFeeAmount { get; set; }

        public float ChargedAmount { get; set; }

        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;
    }
}
