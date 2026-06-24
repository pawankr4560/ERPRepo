namespace WebApp.Model.Transaction
{
    public class LoanDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string LoanNumber { get; set; } = string.Empty;

        public decimal LoanAmount { get; set; }

        public decimal Rate { get; set; }

        public decimal EMI { get; set; }

        public bool InterestCalculationType { get; set; }

        public int Tenure { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? ApprovedAtUtc { get; set; }

        public string? ApprovedByUserId { get; set; }

        public DateTime? RejectedAtUtc { get; set; }

        public string? RejectedByUserId { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime UpdatedDateTime { get; set; }

        public int CreatedBy { get; set; }

        public int UpdatedBy { get; set; }

        public LoanCustomerDetailRequestModel? CustomerDetail { get; set; }

        public List<LoanEMIScheduleDto> EmiSchedules { get; set; } = [];

        public List<LoanPaymentDto> Payments { get; set; } = [];
    }
}
