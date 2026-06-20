namespace WebApp.Model.Transaction
{
    public class LoanDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string LoanNumber { get; set; } = string.Empty;

        public double LoanAmount { get; set; }

        public double Rate { get; set; }

        public double EMI { get; set; }

        public int Tenure { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool Active { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime UpdatedDateTime { get; set; }

        public int CreatedBy { get; set; }

        public int UpdatedBy { get; set; }
    }
}
