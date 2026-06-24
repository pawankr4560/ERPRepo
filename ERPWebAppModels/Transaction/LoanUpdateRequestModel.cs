namespace WebApp.Model.Transaction
{
    public class LoanUpdateRequestModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal EMI { get; set; }
        public decimal Rate { get; set; }
        public bool InterestCalculationType { get; set; }
        [System.ComponentModel.DataAnnotations.Range(
            6,
            600,
            ErrorMessage = "Tenure must be between 6 and 600 months.")]
        public int Tenure { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
