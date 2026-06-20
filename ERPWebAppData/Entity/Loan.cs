using System.ComponentModel.DataAnnotations;

namespace WebApp.Data.Entity
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string LoanNumber { get; set; } = string.Empty;

        public double LoanAmount { get; set; }

        public double Rate { get; set; }

        public int Tenure { get; set; }

        public double EMI { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = "Pending";

        public bool Active { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime F_Created_Date_Time { get; set; }

        public DateTime F_Updated_Date_Time { get; set; }

        public int F_User_Index_Created { get; set; }
    }
}
