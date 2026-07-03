using AutoMapper;
using ERPWebAppModels.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApp.Data;

namespace ERPWebAppService.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IMapper _mapper;
        private readonly WebAppDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IMapper mapper,
            WebAppDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _configuration = configuration;
        }
        public async Task<UserPaymentsResponseDto> GetUserPaymentsAsync(string userId)
        {
            var loans = await _dbContext.Loan
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.LoanNumber
                })
                .ToListAsync();

            if (!loans.Any())
            {
                return new UserPaymentsResponseDto();
            }

            var loanIds = loans.Select(x => x.Id).ToList();

            var nextDue = await _dbContext.LoanEMISchedule
                .Where(x =>
                    loanIds.Contains(x.LoanId) &&
                    x.Active &&
                    !x.IsDeleted &&
                    !x.IsPaid)
                .OrderBy(x => x.DueDate)
                .Select(x => new
                {
                    x.LoanId,
                    x.Id,
                    x.EMIAmount,
                    x.DueDate
                })
                .FirstOrDefaultAsync();

            var payments = await (
                from payment in _dbContext.LoanPayment
                join loan in _dbContext.Loan
                    on payment.LoanId equals loan.Id
                where loan.UserId == userId
                      && !loan.IsDeleted
                      && !payment.IsDeleted
                orderby payment.PaymentDate descending
                select new PaymentHistoryDto
                {
                    Title = "EMI payment",
                    Amount = payment.AmountPaid,
                    Status = "Completed",
                    PaymentDate = payment.PaymentDate,
                    ApplicationId = loan.LoanNumber,
                    TransactionId = payment.TransactionId
                }
            ).ToListAsync();

            var response = new UserPaymentsResponseDto
            {
                Payments = payments
            };

            if (nextDue != null)
            {
                var loan = loans.FirstOrDefault(x => x.Id == nextDue.LoanId);

                response.NextDuePayment = new NextDuePaymentDto
                {
                    Title = "EMI payment",
                    Amount = nextDue.EMIAmount,
                    Status = "Pending",
                    DueDate = nextDue.DueDate,
                    ApplicationId = loan?.LoanNumber ?? string.Empty,
                    LoanId = loan.Id,
                    ScheduleId =nextDue.Id
                };
            }

            return response;
        }
    }
}
