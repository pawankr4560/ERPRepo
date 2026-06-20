using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class LoanEMISchedule
    {
        [Key]
        [Column("F_Schedule_Index")]
        public int Id { get; set; }

        [Column("F_Loan_Index")]
        public int LoanId { get; set; }

        [Column("F_Installment_No")]
        public int InstallmentNo { get; set; }

        [Column("F_Due_Date")]
        public DateTime DueDate { get; set; }

        [Column("F_EMI_Amount")]
        public decimal EMIAmount { get; set; }

        [Column("F_Principal_Amount")]
        public decimal PrincipalAmount { get; set; }

        [Column("F_Interest_Amount")]
        public decimal InterestAmount { get; set; }

        [Column("F_Outstanding_Balance")]
        public decimal OutstandingBalance { get; set; }

        [Column("F_Is_Paid")]
        public bool IsPaid { get; set; }

        [Column("F_Paid_Date")]
        public DateTime? PaidDate { get; set; }

        [Column("F_Active")]
        public bool Active { get; set; }

        [Column("F_Is_Deleted")]
        public bool IsDeleted { get; set; }

        [Column("F_Created_Date_Time")]
        public DateTime F_Created_Date_Time { get; set; }

        [Column("F_Updated_Date_Time")]
        public DateTime? F_Updated_Date_Time { get; set; }

        [Column("F_User_Index_Created")]
        public int? F_User_Index_Created { get; set; }
    }
}
