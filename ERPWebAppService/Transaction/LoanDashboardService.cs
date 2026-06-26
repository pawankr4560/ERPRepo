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
            join user in _dbContext.Users.AsNoTracking()
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
            join user in _dbContext.Users.AsNoTracking()
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

        var activeLoanRows = await (
            from loan in _dbContext.Loan.AsNoTracking()
            join user in _dbContext.Users.AsNoTracking()
                on loan.UserId equals user.Id
            where !loan.IsDeleted
                  && loan.Active
                  && loan.Status == "Active"
            orderby loan.F_Created_Date_Time descending, loan.Id descending
            select new
            {
                loan.Id,
                loan.LoanNumber,
                CustomerName = (user.FirstName + " " + user.LastName).Trim(),
                loan.LoanAmount,
                loan.Rate,
                loan.EMI,
                loan.Tenure,
                loan.StartDate
            })
            .Take(6)
            .ToListAsync(cancellationToken);

        var activeLoanIds = activeLoanRows.Select(loan => loan.Id).ToList();
        var activeLoanSchedules = activeLoanIds.Count == 0
            ? []
            : await _dbContext.LoanEMISchedule
                .AsNoTracking()
                .Where(schedule =>
                    activeLoanIds.Contains(schedule.LoanId)
                    && !schedule.IsDeleted)
                .Select(schedule => new
                {
                    schedule.LoanId,
                    schedule.InstallmentNo,
                    schedule.IsPaid,
                    schedule.OutstandingBalance
                })
                .ToListAsync(cancellationToken);

        var schedulesByLoanId = activeLoanSchedules
            .GroupBy(schedule => schedule.LoanId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var activeLoanSummaries = activeLoanRows
            .Select(loan =>
            {
                schedulesByLoanId.TryGetValue(loan.Id, out var schedules);
                schedules ??= [];

                var totalInstallments = schedules.Count > 0
                    ? schedules.Count
                    : loan.Tenure;
                var paidInstallments = schedules.Count(schedule => schedule.IsPaid);
                var nextUnpaid = schedules
                    .Where(schedule => !schedule.IsPaid)
                    .OrderBy(schedule => schedule.InstallmentNo)
                    .FirstOrDefault();
                var outstandingBalance = nextUnpaid?.OutstandingBalance
                    ?? (paidInstallments >= totalInstallments
                        ? 0
                        : loan.LoanAmount);

                return new LoanDashboardActiveLoanDto
                {
                    LoanId = loan.Id,
                    LoanNumber = loan.LoanNumber,
                    CustomerName = loan.CustomerName,
                    LoanAmount = loan.LoanAmount,
                    Rate = loan.Rate,
                    Emi = loan.EMI,
                    TenureMonths = loan.Tenure,
                    PaidInstallments = paidInstallments,
                    TotalInstallments = totalInstallments,
                    MonthsRemaining = Math.Max(0, totalInstallments - paidInstallments),
                    OutstandingBalance = Math.Max(0, outstandingBalance),
                    ProgressPercentage = totalInstallments > 0
                        ? Math.Round((decimal)paidInstallments / totalInstallments * 100, 2)
                        : 0
                };
            })
            .ToList();

        var totalPortfolio = Convert.ToDecimal(loanStats?.TotalPortfolio ?? 0);
        var totalLoans = loanStats?.TotalLoans ?? 0;
        var activeLoans = loanStats?.ActiveLoans ?? 0;
        var pendingLoans = loanStats?.PendingLoans ?? 0;
        var completedLoans = loanStats?.CompletedLoans ?? 0;
        var outstandingPortfolio = Math.Max(0, totalPortfolio - totalCollected);
        var collectionRate = totalPortfolio > 0
            ? Math.Min(100, totalCollected / totalPortfolio * 100)
            : 0;
        var overdueInstallmentCount = overdueStats?.Count ?? 0;
        var creditScore = Math.Clamp(
            600
            + (int)Math.Round(collectionRate * 2)
            + completedLoans * 5
            - overdueInstallmentCount * 10
            - pendingLoans * 3,
            300,
            850);
        var paymentHistoryRating = overdueInstallmentCount == 0 && collectionRate >= 90
            ? "Excellent"
            : overdueInstallmentCount <= 2 && collectionRate >= 70
                ? "Good"
                : overdueInstallmentCount <= 5
                    ? "Fair"
                    : "Needs attention";
        var averageLoanAgeYears = activeLoanRows.Count > 0
            ? Math.Round(
                activeLoanRows.Average(loan =>
                    (today - loan.StartDate.Date).TotalDays) / 365,
                1)
            : 0;

        stopwatch.Stop();

        return new LoanDashboardDto
        {
            TotalPortfolio = totalPortfolio,
            TotalCollected = totalCollected,
            OutstandingPortfolio = outstandingPortfolio,
            OverdueAmount = overdueStats?.Amount ?? 0,
            TotalLoans = totalLoans,
            ActiveLoans = activeLoans,
            PendingLoans = pendingLoans,
            CompletedLoans = completedLoans,
            OtherLoans = Math.Max(
                0,
                totalLoans - activeLoans - pendingLoans - completedLoans),
            OverdueInstallmentCount = overdueInstallmentCount,
            CollectionRate = collectionRate,
            QueryDurationMs = stopwatch.ElapsedMilliseconds,
            GeneratedAtUtc = DateTime.UtcNow,
            CreditScore = creditScore,
            CreditScoreChange = Math.Clamp(completedLoans * 3 - overdueInstallmentCount * 4, -50, 50),
            CreditUtilization = totalPortfolio > 0
                ? Math.Round(outstandingPortfolio / totalPortfolio * 100, 2)
                : 0,
            AverageLoanAgeYears = Convert.ToDecimal(averageLoanAgeYears),
            HardInquiries = pendingLoans,
            PaymentHistoryRating = paymentHistoryRating,
            UpcomingInstallments = upcomingInstallments,
            RecentPayments = recentPayments,
            ActiveLoanSummaries = activeLoanSummaries
        };
    }
}
