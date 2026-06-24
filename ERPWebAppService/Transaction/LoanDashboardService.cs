using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction;

public class LoanDashboardService : ILoanDashboardService
{
    private static readonly string[] CompletedStatuses = ["Completed", "Closed", "Paid"];
    private static readonly string[] SuccessfulPaymentStatuses = ["Success", "Paid"];

    private readonly WebAppDbContext _dbContext;

    public LoanDashboardService(WebAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoanDashboardDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var today = DateTime.Today;
        var upcomingCutoff = today.AddDays(30);

        var loanStats = await _dbContext.Loan
            .AsNoTracking()
            .Where(loan => !loan.IsDeleted)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalLoans = group.Count(),
                TotalPortfolio = group
                    .Where(loan =>
                        loan.Status != "Pending"
                        && loan.Status != "Rejected")
                    .Sum(loan => loan.LoanAmount),
                ActiveLoans = group.Count(loan =>
                    loan.Active
                    && loan.Status == "Active"),
                PendingLoans = group.Count(loan =>
                    loan.Status == "Pending"),
                CompletedLoans = group.Count(loan =>
                    CompletedStatuses.Contains(loan.Status))
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalCollected = await _dbContext.LoanPayment
            .AsNoTracking()
            .Where(payment =>
                !payment.IsDeleted
                && SuccessfulPaymentStatuses.Contains(payment.PaymentStatus))
            .SumAsync(
                payment => (decimal?)payment.AmountPaid,
                cancellationToken) ?? 0;

        var overdueStats = await (
            from schedule in _dbContext.LoanEMISchedule.AsNoTracking()
            join loan in _dbContext.Loan.AsNoTracking()
                on schedule.LoanId equals loan.Id
            where !schedule.IsDeleted
                  && !schedule.IsPaid
                  && schedule.DueDate < today
                  && !loan.IsDeleted
                  && loan.Active
                  && loan.Status == "Active"
            select schedule)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Count = group.Count(),
                Amount = group.Sum(schedule => schedule.EMIAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var upcomingInstallments = await (
            from schedule in _dbContext.LoanEMISchedule.AsNoTracking()
            join loan in _dbContext.Loan.AsNoTracking()
                on schedule.LoanId equals loan.Id
            join user in _dbContext.UserDetails.AsNoTracking()
                on loan.UserId equals user.Id
            where !schedule.IsDeleted
                  && !schedule.IsPaid
                  && !loan.IsDeleted
                  && loan.Active
                  && loan.Status == "Active"
                  && schedule.DueDate >= today
                  && schedule.DueDate <= upcomingCutoff
            orderby schedule.DueDate, schedule.InstallmentNo
            select new LoanDashboardInstallmentDto
            {
                Id = schedule.Id,
                LoanId = loan.Id,
                LoanNumber = loan.LoanNumber,
                CustomerName = (user.FirstName + " " + user.LastName).Trim(),
                InstallmentNo = schedule.InstallmentNo,
                DueDate = schedule.DueDate,
                EmiAmount = schedule.EMIAmount
            })
            .Take(6)
            .ToListAsync(cancellationToken);

        var recentPayments = await (
            from payment in _dbContext.LoanPayment.AsNoTracking()
            join loan in _dbContext.Loan.AsNoTracking()
                on payment.LoanId equals loan.Id
            join user in _dbContext.UserDetails.AsNoTracking()
                on loan.UserId equals user.Id
            where !payment.IsDeleted
                  && !loan.IsDeleted
                  && loan.Status != "Pending"
                  && loan.Status != "Rejected"
            orderby payment.PaymentDate descending, payment.Id descending
            select new LoanDashboardPaymentDto
            {
                Id = payment.Id,
                LoanId = loan.Id,
                LoanNumber = loan.LoanNumber,
                CustomerName = (user.FirstName + " " + user.LastName).Trim(),
                AmountPaid = payment.AmountPaid,
                PaymentDate = payment.PaymentDate,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId
            })
            .Take(6)
            .ToListAsync(cancellationToken);

        var totalPortfolio = Convert.ToDecimal(loanStats?.TotalPortfolio ?? 0);
        var totalLoans = loanStats?.TotalLoans ?? 0;
        var activeLoans = loanStats?.ActiveLoans ?? 0;
        var pendingLoans = loanStats?.PendingLoans ?? 0;
        var completedLoans = loanStats?.CompletedLoans ?? 0;

        stopwatch.Stop();

        return new LoanDashboardDto
        {
            TotalPortfolio = totalPortfolio,
            TotalCollected = totalCollected,
            OutstandingPortfolio = Math.Max(0, totalPortfolio - totalCollected),
            OverdueAmount = overdueStats?.Amount ?? 0,
            TotalLoans = totalLoans,
            ActiveLoans = activeLoans,
            PendingLoans = pendingLoans,
            CompletedLoans = completedLoans,
            OtherLoans = Math.Max(
                0,
                totalLoans - activeLoans - pendingLoans - completedLoans),
            OverdueInstallmentCount = overdueStats?.Count ?? 0,
            CollectionRate = totalPortfolio > 0
                ? Math.Min(100, totalCollected / totalPortfolio * 100)
                : 0,
            QueryDurationMs = stopwatch.ElapsedMilliseconds,
            GeneratedAtUtc = DateTime.UtcNow,
            UpcomingInstallments = upcomingInstallments,
            RecentPayments = recentPayments
        };
    }
}
