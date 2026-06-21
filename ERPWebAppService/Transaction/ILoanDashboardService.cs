using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction;

public interface ILoanDashboardService
{
    Task<LoanDashboardDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
