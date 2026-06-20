using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class LoanPayment
    {
        [Key]
        [Column("F_Payment_Index")]
        public int Id { get; set; }

        [Required]
        [Column("F_Loan_Index")]
        public int LoanId { get; set; }

        [Required]
        [Column("F_Schedule_Index")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("F_Amount_Paid", TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Required]
        [Column("F_Payment_Date")]
        public DateTime PaymentDate { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("F_Transaction_Id")]
        public string TransactionId { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("F_Payment_Mode")]
        public string? PaymentMode { get; set; }

        [MaxLength(50)]
        [Column("F_Payment_Status")]
        public string PaymentStatus { get; set; } = "Success";

        [MaxLength(500)]
        [Column("F_Remarks")]
        public string? Remarks { get; set; }

        [Required]
        [Column("F_Active")]
        public bool Active { get; set; } = true;

        [Required]
        [Column("F_Is_Deleted")]
        public bool IsDeleted { get; set; } = false;

        [Required]
        [Column("F_Created_Date_Time")]
        public DateTime F_Created_Date_Time { get; set; }

        [Column("F_Updated_Date_Time")]
        public DateTime? F_Updated_Date_Time { get; set; }

        [Column("F_User_Index_Created")]
        public int? F_User_Index_Created { get; set; }

        [Column("F_User_Index_Update")]
        public int? F_User_Index_Update { get; set; }

        [ForeignKey(nameof(LoanId))]
        public virtual Loan? Loan { get; set; }

    }
}
