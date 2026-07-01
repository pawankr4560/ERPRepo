using ERPWebAppModels.Auth;
using ERPWebAppModels.Dashbord;
using Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entity;

namespace ERPWebAppService.Dashbord
{
    public class DashbordService : IDashbordService
    {
        private readonly WebAppDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public DashbordService(
            WebAppDbContext dbContext,
            UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<DashboardDto?> GetDashboardAsync(string userId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var notificationsCount = await _dbContext.Notification
                .CountAsync(x =>
                    x.UserId == userId &&
                    !x.IsRead &&
                    !x.IsDeleted);

            var offer = await _dbContext.PreApprovedOffer
                .Where(x =>
                    x.UserId == userId &&
                    x.IsAvailable &&
                    !x.IsDeleted &&
                    x.ValidFrom <= DateTime.UtcNow &&
                    x.ValidTo >= DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefaultAsync();

            var activeLoans = await _dbContext.Loan
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsDeleted &&
                    x.Active)
                .OrderByDescending(x => x.F_Created_Date_Time)
                .ToListAsync();

            var activeLoanIds = activeLoans.Select(x => x.Id).ToList();

            var nextEmi = await _dbContext.LoanEMISchedule
                .Where(x =>
                    activeLoanIds.Contains(x.LoanId) &&
                    !x.IsPaid)
                .OrderBy(x => x.DueDate)
                .FirstOrDefaultAsync();

            var emiLoan = nextEmi == null
                ? null
                : activeLoans.FirstOrDefault(x => x.Id == nextEmi.LoanId);

            return new DashboardDto
            {
                User = new UserRes
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    Phone = user.PhoneNumber ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "User"
                },

                NotificationsCount = notificationsCount,

                PreApprovedOffer = offer == null ? null : new DashboardPreApprovedOfferDto
                {
                    IsAvailable = offer.IsAvailable,
                    MaxAmount = offer.MaxAmount,
                    InterestRate = offer.InterestRate,
                    AmountLabel = $"Up to ₹{offer.MaxAmount:N0}",
                    RateLabel = $"Interest rate from {offer.InterestRate}% p.a.",
                    OfferId = offer.OfferCode
                },

                EmiSummary = nextEmi == null || emiLoan == null ? null : new DashboardEmiSummaryDto
                {
                    Amount = nextEmi.EMIAmount,
                    DueDate = nextEmi.DueDate,
                    LoanId = emiLoan.LoanNumber
                },

                ActiveApplications = activeLoans.Select(x => new DashboardActiveApplicationDto
                {
                    Id = x.LoanNumber,
                    //Type = x.LoanType,
                    Amount = Convert.ToDecimal(x.LoanAmount),
                    Status = x.Status,
                    StepsCompleted = GetStepsCompleted(x.Status),
                    ProgressLabel = x.Status,
                    SubmittedDate = x.F_Created_Date_Time
                }).ToList()
            };
        }
        private static int GetStepsCompleted(string status)
        {
            return status switch
            {
                "Submitted" => 1,
                "Under review" => 2,
                "Approved" => 3,
                "Disbursed" => 4,
                "Rejected" => 1,
                _ => 0
            };
        }
    }
}
