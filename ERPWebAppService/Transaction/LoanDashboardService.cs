using System.Diagnostics;
using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction;

public class LoanDashboardService : ILoanDashboardService
{
    private static readonly string[] CompletedStatuses = ["Completed", "Closed", "Paid"];
    private static readonly string[] SuccessfulPaymentStatuses = ["Success", "Paid"];
    private readonly MongoDbContext _context;

    public LoanDashboardService(MongoDbContext context) => _context = context;

    public async Task<LoanDashboardDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var today = DateTime.Today;
        var loans = await _context.Loans.Find(x => !x.IsDeleted).ToListAsync(cancellationToken);
        var loanById = loans.ToDictionary(x => x.Id);
        var activeLoanIds = loans.Where(x => x.Active && x.Status == "Active")
            .Select(x => x.Id).ToHashSet();
        var payments = await _context.LoanPayments
            .Find(x => !x.IsDeleted).ToListAsync(cancellationToken);
        var schedules = await _context.LoanEMISchedules
            .Find(x => !x.IsDeleted).ToListAsync(cancellationToken);
        var users = await _context.Users.Find(x => !x.IsDeleted).ToListAsync(cancellationToken);
        var userById = users.ToDictionary(x => x.Id);

        var totalPortfolio = Convert.ToDecimal(loans
            .Where(x => x.Status is not "Pending" and not "Rejected").Sum(x => x.LoanAmount));
        var totalCollected = payments
            .Where(x => SuccessfulPaymentStatuses.Contains(x.PaymentStatus))
            .Sum(x => x.AmountPaid);
        var overdue = schedules.Where(x =>
            !x.IsPaid && x.DueDate < today && activeLoanIds.Contains(x.LoanId)).ToList();

        var upcoming = schedules.Where(x =>
                !x.IsPaid && x.DueDate >= today && x.DueDate <= today.AddDays(30)
                && activeLoanIds.Contains(x.LoanId))
            .OrderBy(x => x.DueDate).ThenBy(x => x.InstallmentNo).Take(6)
            .Select(x =>
            {
                var loan = loanById[x.LoanId];
                userById.TryGetValue(loan.UserId, out var user);
                return new LoanDashboardInstallmentDto
                {
                    Id = x.Id, LoanId = loan.Id, LoanNumber = loan.LoanNumber,
                    CustomerName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim(),
                    InstallmentNo = x.InstallmentNo, DueDate = x.DueDate, EmiAmount = x.EMIAmount
                };
            }).ToList();

        var recent = payments.Where(x =>
                loanById.TryGetValue(x.LoanId, out var loan)
                && loan.Status is not "Pending" and not "Rejected")
            .OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.Id).Take(6)
            .Select(x =>
            {
                var loan = loanById[x.LoanId];
                userById.TryGetValue(loan.UserId, out var user);
                return new LoanDashboardPaymentDto
                {
                    Id = x.Id, LoanId = loan.Id, LoanNumber = loan.LoanNumber,
                    CustomerName = user == null ? "" : $"{user.FirstName} {user.LastName}".Trim(),
                    AmountPaid = x.AmountPaid, PaymentDate = x.PaymentDate,
                    PaymentStatus = x.PaymentStatus, TransactionId = x.TransactionId
                };
            }).ToList();

        stopwatch.Stop();
        var active = activeLoanIds.Count;
        var pending = loans.Count(x => x.Status == "Pending");
        var completed = loans.Count(x => CompletedStatuses.Contains(x.Status));
        return new LoanDashboardDto
        {
            TotalPortfolio = totalPortfolio,
            TotalCollected = totalCollected,
            OutstandingPortfolio = Math.Max(0, totalPortfolio - totalCollected),
            OverdueAmount = overdue.Sum(x => x.EMIAmount),
            TotalLoans = loans.Count,
            ActiveLoans = active,
            PendingLoans = pending,
            CompletedLoans = completed,
            OtherLoans = Math.Max(0, loans.Count - active - pending - completed),
            OverdueInstallmentCount = overdue.Count,
            CollectionRate = totalPortfolio > 0
                ? Math.Min(100, totalCollected / totalPortfolio * 100) : 0,
            QueryDurationMs = stopwatch.ElapsedMilliseconds,
            GeneratedAtUtc = DateTime.UtcNow,
            UpcomingInstallments = upcoming,
            RecentPayments = recent
        };
    }
}
