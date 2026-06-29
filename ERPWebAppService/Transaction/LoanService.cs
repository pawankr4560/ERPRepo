using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;
using WebApp.Service.Email;

namespace WebApp.Service.Transaction
{
    public class LoanService : ILoanService
    {
        private const string PendingStatus = "Pending";
        private const string ActiveStatus = "Active";
        private const string RejectedStatus = "Rejected";

        private readonly WebAppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<LoanService> _logger;

        public LoanService(
            WebAppDbContext dbContext,
            IMapper mapper,
            IEmailService emailService,
            ILogger<LoanService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
        }

        #region Get All

        public async Task<List<LoanDto>> LoanList()
        {
            try
            {
                return await (
                    from loan in _dbContext.Loan
                    join user in _dbContext.Users
                        on loan.UserId equals user.Id
                    where !loan.IsDeleted
                    select new LoanDto
                    {
                        Id = loan.Id,
                        UserId = loan.UserId,
                        UserName = $"{user.FirstName} {user.LastName}",
                        LoanNumber = loan.LoanNumber,
                        LoanAmount = loan.LoanAmount,
                        Rate = loan.Rate,
                        EMI = loan.EMI,
                        InterestCalculationType = loan.IsReducingInterest,
                        Tenure = loan.Tenure,
                        StartDate = loan.StartDate,
                        EndDate = loan.EndDate,
                        Status = loan.Status,
                        Active = loan.Active,
                        ApprovedAtUtc = loan.ApprovedAtUtc,
                        ApprovedByUserId = loan.ApprovedByUserId,
                        RejectedAtUtc = loan.RejectedAtUtc,
                        RejectedByUserId = loan.RejectedByUserId,
                        CreatedDateTime = loan.F_Created_Date_Time,
                        UpdatedDateTime = loan.F_Updated_Date_Time
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching loan list.", ex);
            }
        }

        #endregion

        #region Get By Id

        public async Task<LoanDto?> GetLoanById(int id)
        {
            try
            {
                var loan = await (
                    from item in _dbContext.Loan.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking()
                        on item.UserId equals user.Id
                    where item.Id == id && !item.IsDeleted
                    select new LoanDto
                    {
                        Id = item.Id,
                        UserId = item.UserId,
                        UserName = $"{user.FirstName} {user.LastName}",
                        LoanNumber = item.LoanNumber,
                        LoanAmount = item.LoanAmount,
                        Rate = item.Rate,
                        EMI = item.EMI,
                        InterestCalculationType = item.IsReducingInterest,
                        Tenure = item.Tenure,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        Status = item.Status,
                        Active = item.Active,
                        ApprovedAtUtc = item.ApprovedAtUtc,
                        ApprovedByUserId = item.ApprovedByUserId,
                        RejectedAtUtc = item.RejectedAtUtc,
                        RejectedByUserId = item.RejectedByUserId,
                        CreatedDateTime = item.F_Created_Date_Time,
                        UpdatedDateTime = item.F_Updated_Date_Time,
                        CreatedBy = item.F_User_Index_Created
                    })
                    .FirstOrDefaultAsync();

                if (loan == null)
                    return null;

                loan.CustomerDetail = await _dbContext.LoanCustomerDetail
                    .AsNoTracking()
                    .Where(x => x.LoanId == id && !x.IsDeleted)
                    .Select(x => new LoanCustomerDetailRequestModel
                    {
                        CustomerAadhaarNo = x.CustomerAadhaarNo,
                        CustomerMobileNo = x.CustomerMobileNo,
                        CustomerAddress = x.CustomerAddress,
                        CustomerCity = x.CustomerCity,
                        CustomerState = x.CustomerState,
                        CustomerPinCode = x.CustomerPinCode,
                        GuarantorName = x.GuarantorName,
                        GuarantorAadhaarNo = x.GuarantorAadhaarNo,
                        GuarantorMobileNo = x.GuarantorMobileNo,
                        GuarantorAddress = x.GuarantorAddress,
                        GuarantorRelationship = x.GuarantorRelationship
                    })
                    .FirstOrDefaultAsync();

                loan.EmiSchedules = await _dbContext.LoanEMISchedule
                    .AsNoTracking()
                    .Where(x => x.LoanId == id && !x.IsDeleted)
                    .OrderBy(x => x.InstallmentNo)
                    .Select(x => new LoanEMIScheduleDto
                    {
                        Id = x.Id,
                        LoanId = x.LoanId,
                        InstallmentNo = x.InstallmentNo,
                        DueDate = x.DueDate,
                        EMIAmount = x.EMIAmount,
                        PrincipalAmount = x.PrincipalAmount,
                        InterestAmount = x.InterestAmount,
                        OutstandingBalance = x.OutstandingBalance,
                        IsPaid = x.IsPaid,
                        PaidDate = x.PaidDate
                    })
                    .ToListAsync();

                loan.Payments = await _dbContext.LoanPayment
                    .AsNoTracking()
                    .Where(x => x.LoanId == id && !x.IsDeleted)
                    .OrderByDescending(x => x.PaymentDate)
                    .Select(x => new LoanPaymentDto
                    {
                        Id = x.Id,
                        LoanId = x.LoanId,
                        ScheduleId = x.ScheduleId,
                        AmountPaid = x.AmountPaid,
                        PaymentDate = x.PaymentDate,
                        TransactionId = x.TransactionId,
                        PaymentMode = x.PaymentMode,
                        PaymentStatus = x.PaymentStatus,
                        Remarks = x.Remarks
                    })
                    .ToListAsync();

                return loan;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while fetching loan with Id {id}.", ex);
            }
        }

        #endregion

        #region Add

        public async Task<bool> AddLoan(LoanRequestModel model)
        {
            try
            {
                var loanExists = await _dbContext.Loan
                    .AnyAsync(x =>
                        x.LoanNumber == model.LoanNumber);

                if (loanExists)
                    throw new Exception("This Loan number already exists.");

                var entity = _mapper.Map<Data.Entity.Loan>(model);

                entity.Status = PendingStatus;
                entity.Active = false;
                entity.IsReducingInterest = model.interestCalculationType;
                entity.IsDeleted = false;
                entity.F_Created_Date_Time = DateTime.UtcNow;
                entity.F_Updated_Date_Time = DateTime.UtcNow;

                await _dbContext.Loan.AddAsync(entity);

                var isSavedSuccessfully =
                    await _dbContext.SaveChangesAsync() > 0;

                var customerDetail = new LoanCustomerDetail
                {
                    LoanId = entity.Id,

                    CustomerAadhaarNo = model.CustomerDetail.CustomerAadhaarNo,
                    CustomerMobileNo = model.CustomerDetail.CustomerMobileNo,
                    CustomerAddress = model.CustomerDetail.CustomerAddress,
                    CustomerCity = model.CustomerDetail.CustomerCity,
                    CustomerState = model.CustomerDetail.CustomerState,
                    CustomerPinCode = model.CustomerDetail.CustomerPinCode,

                    GuarantorName = model.CustomerDetail.GuarantorName,
                    GuarantorAadhaarNo = model.CustomerDetail.GuarantorAadhaarNo,
                    GuarantorMobileNo = model.CustomerDetail.GuarantorMobileNo,
                    GuarantorAddress = model.CustomerDetail.GuarantorAddress,
                    GuarantorRelationship = model.CustomerDetail.GuarantorRelationship,

                    IsDeleted = false,
                    F_Created_Date_Time = DateTime.UtcNow
                };

                await _dbContext.LoanCustomerDetail.AddAsync(customerDetail);
                await _dbContext.SaveChangesAsync();
                await SendLoanEmailAsync(entity.Id, "created");
                return isSavedSuccessfully;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while creating loan.", ex);
            }
        }

        #endregion

        #region Generate Loan Number

        public async Task<CreateLoanDTO> GetLoanNumber()
        {
            try
            {
                var currentYear = DateTime.UtcNow.Year;

                var lastLoanNumber = await _dbContext.Loan
                    .Where(x =>
                        x.LoanNumber.StartsWith($"{currentYear}-GKFIN-"))
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.LoanNumber)
                    .FirstOrDefaultAsync();

                int nextSequence = 1;

                if (!string.IsNullOrWhiteSpace(lastLoanNumber))
                {
                    var sequencePart = lastLoanNumber.Split('-').Last();

                    if (int.TryParse(sequencePart, out int sequence))
                    {
                        nextSequence = sequence + 1;
                    }
                }

                var customers = await (
                    from user in _dbContext.Users
                    join userRole in _dbContext.UserRoles
                        on user.Id equals userRole.UserId
                    join role in _dbContext.Roles
                        on userRole.RoleId equals role.Id
                    where !user.IsDeleted
                          && role.Name.ToUpper() == "USER"
                    orderby user.FirstName
                    select new CustomerDropdownDto
                    {
                        Id = user.Id,
                        CustomerName = $"{user.FirstName} {user.LastName}"
                    })
                    .Distinct()
                    .ToListAsync();

                return new CreateLoanDTO
                {
                    LoanNumber = $"{currentYear}-GKFIN-{nextSequence:D5}",
                    CustomerList = customers
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error while generating loan number.", ex);
            }
        }

        #endregion

        #region Update

        public async Task<bool> UpdateLoan(LoanUpdateRequestModel model)
        {
            try
            {
                var loan = await _dbContext.Loan
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.Id &&
                        !x.IsDeleted);

                if (loan == null)
                    return false;

                if (loan.Status == ActiveStatus)
                    throw new InvalidOperationException("Approved loans cannot be updated.");

                var hasPaidEmi = await _dbContext.LoanEMISchedule
                    .AnyAsync(x =>
                        x.LoanId == loan.Id &&
                        x.IsPaid &&
                        !x.IsDeleted);

                if (hasPaidEmi)
                {
                    throw new Exception("Loan cannot be updated because EMI payment already exists.");
                }

                loan.UserId = model.UserId;
                loan.LoanNumber = model.LoanNumber;
                loan.LoanAmount = model.LoanAmount;
                loan.Rate = model.Rate;
                loan.EMI = model.EMI;
                loan.IsReducingInterest = model.InterestCalculationType;
                loan.Tenure = model.Tenure;
                loan.StartDate = model.StartDate;
                loan.EndDate = model.EndDate;
                loan.F_Updated_Date_Time = DateTime.UtcNow;

                var oldSchedules = await _dbContext.LoanEMISchedule
                    .Where(x => x.LoanId == loan.Id && !x.IsDeleted)
                    .ToListAsync();

                if (oldSchedules.Any())
                {
                    _dbContext.LoanEMISchedule.RemoveRange(oldSchedules);
                }

                await _dbContext.SaveChangesAsync();

                if (loan.Status == ActiveStatus)
                {
                    if (loan.IsReducingInterest)
                    {
                        await GenerateReducingEMIScheduleAsync(loan);
                    }
                    else
                    {
                        await GenerateEMIScheduleAsync(loan);
                    }
                }

                await SendLoanEmailAsync(loan.Id, "updated");
                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating loan Id {model.Id}.", ex);
            }
        }

        #endregion

        #region Approval

        public async Task<bool> ApproveLoan(int id, string approvedByUserId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var loan = await _dbContext.Loan
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (loan == null)
                return false;

            if (loan.Status != PendingStatus)
                throw new InvalidOperationException("Only pending loans can be approved.");

            var hasPaidEmi = await _dbContext.LoanEMISchedule
                .AnyAsync(x => x.LoanId == id && x.IsPaid && !x.IsDeleted);

            if (hasPaidEmi)
                throw new InvalidOperationException("A loan with paid installments cannot be approved.");

            loan.Status = ActiveStatus;
            loan.Active = true;
            loan.ApprovedAtUtc = DateTime.UtcNow;
            loan.ApprovedByUserId = approvedByUserId;
            loan.RejectedAtUtc = null;
            loan.RejectedByUserId = null;
            loan.F_Updated_Date_Time = DateTime.UtcNow;

            var legacySchedules = await _dbContext.LoanEMISchedule
                .Where(x => x.LoanId == id && !x.IsDeleted)
                .ToListAsync();

            if (legacySchedules.Count > 0)
            {
                _dbContext.LoanEMISchedule.RemoveRange(legacySchedules);
            }

            await _dbContext.SaveChangesAsync();

            if (loan.IsReducingInterest)
                await GenerateReducingEMIScheduleAsync(loan);
            else
                await GenerateEMIScheduleAsync(loan);

            await transaction.CommitAsync();
            await SendLoanEmailAsync(loan.Id, "approved");
            return true;
        }

        public async Task<bool> RejectLoan(int id, string rejectedByUserId)
        {
            var loan = await _dbContext.Loan
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (loan == null)
                return false;

            if (loan.Status != PendingStatus)
                throw new InvalidOperationException("Only pending loans can be rejected.");

            var hasPaidEmi = await _dbContext.LoanEMISchedule
                .AnyAsync(x => x.LoanId == id && x.IsPaid && !x.IsDeleted);

            if (hasPaidEmi)
                throw new InvalidOperationException("A loan with paid installments cannot be rejected.");

            loan.Status = RejectedStatus;
            loan.Active = false;
            loan.RejectedAtUtc = DateTime.UtcNow;
            loan.RejectedByUserId = rejectedByUserId;
            loan.ApprovedAtUtc = null;
            loan.ApprovedByUserId = null;
            loan.F_Updated_Date_Time = DateTime.UtcNow;

            var saved = await _dbContext.SaveChangesAsync() > 0;
            if (saved)
            {
                await SendLoanEmailAsync(loan.Id, "rejected");
            }

            return saved;
        }

        #endregion

        #region Delete

        public async Task<bool> DeleteLoan(int id)
        {
            try
            {
                var loan = await _dbContext.Loan
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);

                if (loan == null)
                    return false;

                if (loan.Status == ActiveStatus)
                    throw new InvalidOperationException("Approved loans cannot be deleted.");

                loan.IsDeleted = true;
                loan.Active = false;
                loan.F_Updated_Date_Time = DateTime.UtcNow;

                _dbContext.Loan.Update(loan);

                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting loan Id {id}.", ex);
            }
        }

        private async Task GenerateEMIScheduleAsync(Loan loan)
        {
            var schedules = new List<LoanEMISchedule>();

            decimal loanAmount = loan.LoanAmount;
            decimal annualRate = loan.Rate;
            int tenureMonths = loan.Tenure;

            decimal totalInterest = loanAmount * annualRate * tenureMonths / 12 / 100;
            decimal monthlyInterest = totalInterest / tenureMonths;
            decimal monthlyPrincipal = loanAmount / tenureMonths;

            decimal outstandingBalance = loanAmount;

            for (int installmentNo = 1; installmentNo <= tenureMonths; installmentNo++)
            {
                decimal principalAmount = monthlyPrincipal;
                decimal interestAmount = monthlyInterest;
                decimal emiAmount = principalAmount + interestAmount;

                if (installmentNo == tenureMonths)
                {
                    principalAmount = outstandingBalance;
                    emiAmount = principalAmount + interestAmount;
                }

                outstandingBalance -= principalAmount;

                schedules.Add(new LoanEMISchedule
                {
                    LoanId = loan.Id,
                    InstallmentNo = installmentNo,

                    DueDate = loan.StartDate.AddMonths(installmentNo),

                    EMIAmount = Math.Round(emiAmount, 2),
                    PrincipalAmount = Math.Round(principalAmount, 2),
                    InterestAmount = Math.Round(interestAmount, 2),
                    OutstandingBalance = Math.Round(outstandingBalance < 0 ? 0 : outstandingBalance, 2),

                    IsPaid = false,
                    Active = true,
                    IsDeleted = false,
                    F_Created_Date_Time = DateTime.UtcNow,
                    F_Updated_Date_Time = DateTime.UtcNow
                });
            }

            await _dbContext.LoanEMISchedule.AddRangeAsync(schedules);
            await _dbContext.SaveChangesAsync();
        }
        private async Task GenerateReducingEMIScheduleAsync(Loan loan)
        {
            var schedules = new List<LoanEMISchedule>();

            decimal loanAmount = loan.LoanAmount;
            decimal annualRate = loan.Rate;
            int tenureMonths = loan.Tenure;

            decimal monthlyRate = annualRate / 12 / 100;
            decimal outstandingBalance = loanAmount;

            decimal emiAmount;

            if (monthlyRate == 0)
            {
                emiAmount = loanAmount / tenureMonths;
            }
            else
            {
                decimal factor = (decimal)Math.Pow(
                    Convert.ToDouble(1 + monthlyRate),
                    tenureMonths);

                emiAmount = loanAmount * monthlyRate * factor / (factor - 1);
            }

            for (int installmentNo = 1; installmentNo <= tenureMonths; installmentNo++)
            {
                decimal interestAmount = outstandingBalance * monthlyRate;
                decimal principalAmount = emiAmount - interestAmount;

                if (installmentNo == tenureMonths)
                {
                    principalAmount = outstandingBalance;
                    emiAmount = principalAmount + interestAmount;
                }

                outstandingBalance -= principalAmount;

                schedules.Add(new LoanEMISchedule
                {
                    LoanId = loan.Id,
                    InstallmentNo = installmentNo,
                    DueDate = loan.StartDate.AddMonths(installmentNo),

                    EMIAmount = Math.Round(emiAmount, 2),
                    PrincipalAmount = Math.Round(principalAmount, 2),
                    InterestAmount = Math.Round(interestAmount, 2),
                    OutstandingBalance = Math.Round(outstandingBalance < 0 ? 0 : outstandingBalance, 2),

                    IsPaid = false,
                    Active = true,
                    IsDeleted = false,
                    F_Created_Date_Time = DateTime.UtcNow,
                    F_Updated_Date_Time = DateTime.UtcNow
                });

            }

            await _dbContext.LoanEMISchedule.AddRangeAsync(schedules);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SendLoanEmailAsync(int loanId, string action)
        {
            try
            {
                var context = await GetLoanEmailContextAsync(loanId);
                if (context == null || string.IsNullOrWhiteSpace(context.CustomerEmail))
                {
                    _logger.LogWarning("Loan email skipped for loan {LoanId}. Customer email is missing.", loanId);
                    return;
                }

                var subject = action switch
                {
                    "created" => $"Loan application received - {context.LoanNumber}",
                    "updated" => $"Loan application updated - {context.LoanNumber}",
                    "approved" => $"Loan approved - {context.LoanNumber}",
                    "rejected" => $"Loan rejected - {context.LoanNumber}",
                    _ => $"Loan update - {context.LoanNumber}"
                };

                var body = BuildLoanEmailBody(context, action);
                await _emailService.SendEmailAsync(context.CustomerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send {Action} loan email for loan {LoanId}.", action, loanId);
            }
        }

        private async Task<LoanEmailContext?> GetLoanEmailContextAsync(int loanId)
        {
            return await (
                from loan in _dbContext.Loan.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking()
                    on loan.UserId equals user.Id
                where loan.Id == loanId && !loan.IsDeleted
                select new LoanEmailContext
                {
                    CustomerName = $"{user.FirstName} {user.LastName}".Trim(),
                    CustomerEmail = user.Email ?? string.Empty,
                    LoanNumber = loan.LoanNumber,
                    LoanAmount = loan.LoanAmount,
                    EMI = loan.EMI,
                    Tenure = loan.Tenure,
                    Status = loan.Status,
                    StartDate = loan.StartDate,
                    EndDate = loan.EndDate
                })
                .FirstOrDefaultAsync();
        }

        private static string BuildLoanEmailBody(LoanEmailContext context, string action)
        {
            var customerName = string.IsNullOrWhiteSpace(context.CustomerName)
                ? "Customer"
                : context.CustomerName;

            var intro = action switch
            {
                "created" => "Your loan application has been created and is currently under review.",
                "updated" => "Your loan application details have been updated.",
                "approved" => "Good news. Your loan application has been approved.",
                "rejected" => "Your loan application has been reviewed and rejected.",
                _ => "There is an update on your loan application."
            };

            return $@"
                <p>Hello {WebUtility.HtmlEncode(customerName)},</p>
                <p>{WebUtility.HtmlEncode(intro)}</p>
                <table cellpadding=""6"" cellspacing=""0"" style=""border-collapse:collapse"">
                    <tr><td><strong>Loan Number</strong></td><td>{WebUtility.HtmlEncode(context.LoanNumber)}</td></tr>
                    <tr><td><strong>Amount</strong></td><td>INR {context.LoanAmount:N2}</td></tr>
                    <tr><td><strong>EMI</strong></td><td>INR {context.EMI:N2}</td></tr>
                    <tr><td><strong>Tenure</strong></td><td>{context.Tenure} months</td></tr>
                    <tr><td><strong>Status</strong></td><td>{WebUtility.HtmlEncode(context.Status)}</td></tr>
                    <tr><td><strong>Start Date</strong></td><td>{context.StartDate:dd MMM yyyy}</td></tr>
                    <tr><td><strong>End Date</strong></td><td>{context.EndDate:dd MMM yyyy}</td></tr>
                </table>
                <p>Thank you,<br/>GKFIN PVT LTD</p>";
        }

        private sealed class LoanEmailContext
        {
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string LoanNumber { get; set; } = string.Empty;
            public decimal LoanAmount { get; set; }
            public decimal EMI { get; set; }
            public int Tenure { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        #endregion
    }
}
