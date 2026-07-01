namespace ERPWebAppModels.Transaction
{
    public class CreateLoanApplicationResponseDto
    {
        public string ApplicationId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int StepsCompleted { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public int Tenure { get; set; }

        public DateTime SubmittedDate { get; set; }

        public string NextStep { get; set; } = string.Empty;
    }
}
