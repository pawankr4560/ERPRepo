namespace WebApp.Model.Transaction;

public class LoanDashboardDto
{
    public decimal TotalPortfolio { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal OutstandingPortfolio { get; set; }
    public decimal OverdueAmount { get; set; }
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int PendingLoans { get; set; }
    public int CompletedLoans { get; set; }
    public int OtherLoans { get; set; }
    public int OverdueInstallmentCount { get; set; }
    public decimal CollectionRate { get; set; }
    public long QueryDurationMs { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public int CreditScore { get; set; }
    public int CreditScoreChange { get; set; }
    public decimal CreditUtilization { get; set; }
    public decimal AverageLoanAgeYears { get; set; }
    public int HardInquiries { get; set; }
    public string PaymentHistoryRating { get; set; } = "No history";
    public List<LoanDashboardInstallmentDto> UpcomingInstallments { get; set; } = [];
    public List<LoanDashboardPaymentDto> RecentPayments { get; set; } = [];
    public List<LoanDashboardActiveLoanDto> ActiveLoanSummaries { get; set; } = [];
}

public class LoanDashboardInstallmentDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int InstallmentNo { get; set; }
    public DateTime DueDate { get; set; }
    public decimal EmiAmount { get; set; }
}

public class LoanDashboardPaymentDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}

public class LoanDashboardActiveLoanDto
{
    public int LoanId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal Rate { get; set; }
    public decimal Emi { get; set; }
    public int TenureMonths { get; set; }
    public int PaidInstallments { get; set; }
    public int TotalInstallments { get; set; }
    public int MonthsRemaining { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal ProgressPercentage { get; set; }
}
