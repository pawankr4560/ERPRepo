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
    public List<LoanDashboardInstallmentDto> UpcomingInstallments { get; set; } = [];
    public List<LoanDashboardPaymentDto> RecentPayments { get; set; } = [];
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
