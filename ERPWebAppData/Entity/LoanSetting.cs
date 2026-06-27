using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Data.Entity
{
    public class LoanSetting
    {
        [Key]
        public int Id { get; set; }

        public bool InterestCalculationType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BookingPaymentFixedCharge { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal BookingPaymentPercentageCharge { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
