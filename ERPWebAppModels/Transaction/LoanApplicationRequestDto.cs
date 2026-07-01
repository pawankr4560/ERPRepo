namespace ERPWebAppModels.Transaction
{
    public class LoanApplicationRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public DateTime DOB { get; set; }

        public string Address { get; set; } = string.Empty;

        public string PANNumber { get; set; } = string.Empty;

        public string EmploymentType { get; set; } = string.Empty;

        public string EmployerName { get; set; } = string.Empty;

        public decimal MonthlyIncome { get; set; }

        public int WorkExperience { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal AmountRequested { get; set; }

        public int Tenure { get; set; }

        public string Purpose { get; set; } = string.Empty;
    }
}
