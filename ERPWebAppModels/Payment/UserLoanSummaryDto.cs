namespace WebApp.Model.Payment;

public class UserLoanSummaryDto
{
    public int Id { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal Emi { get; set; }
    public int Tenure { get; set; }
    public string Status { get; set; } = string.Empty;
    public int UnpaidInstallments { get; set; }
    public DateTime? NextDueDate { get; set; }
    public decimal? NextEmiAmount { get; set; }
}
