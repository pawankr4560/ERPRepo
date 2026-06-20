using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;
using WebApp.Service.Message;

namespace WebApp.Service.Transaction
{
    public class LoanService : ILoanService
    {
        private readonly WebAppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IMessageService _messageService;

        public LoanService(
            WebAppDbContext dbContext,
            IMapper mapper,
            IMessageService messageService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _messageService = messageService;
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
                        Tenure = loan.Tenure,
                        Active = loan.Active,
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
                var loan = await _dbContext.Loan
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                return loan == null
                    ? null
                    : _mapper.Map<LoanDto>(loan);
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

                entity.Active = true;
                entity.IsDeleted = false;
                entity.F_Created_Date_Time = DateTime.UtcNow;
                entity.F_Updated_Date_Time = DateTime.UtcNow;

                await _dbContext.Loan.AddAsync(entity);

                var isSavedSuccessfully =
                    await _dbContext.SaveChangesAsync() > 0;

                if (isSavedSuccessfully)
                {
                    //var user = await _dbContext.Users
                    //    .FirstOrDefaultAsync(x => x.Id == model.UserId);

                    //if (user != null &&
                    //    !string.IsNullOrWhiteSpace(user.PhoneNumber))
                    //{
                    //    await _messageService.SendLoanSmsAsync(
                    //        user.PhoneNumber,
                    //        entity.LoanNumber,
                    //        entity.LoanAmount.ToString());
                    //}
                    if (model.interestCalculationType)
                        await GenerateReducingEMIScheduleAsync(entity);
                    else
                        await GenerateEMIScheduleAsync(entity);
                }
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

        public async Task<bool> UpdateLoan(LoanRequestModel model)
        {
            try
            {
                var loan = await _dbContext.Loan
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.Id &&
                        !x.IsDeleted);

                if (loan == null)
                    return false;

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
                loan.Tenure = model.Tenure;
                loan.StartDate = model.StartDate;
                loan.EndDate = model.EndDate;
                loan.Status = model.Status;
                loan.Active = model.Active;
                loan.F_Updated_Date_Time = DateTime.UtcNow;

                var oldSchedules = await _dbContext.LoanEMISchedule
                    .Where(x => x.LoanId == loan.Id && !x.IsDeleted)
                    .ToListAsync();

                if (oldSchedules.Any())
                {
                    _dbContext.LoanEMISchedule.RemoveRange(oldSchedules);
                }

                await _dbContext.SaveChangesAsync();

                if (model.interestCalculationType)
                {
                    await GenerateReducingEMIScheduleAsync(loan);
                }
                else
                {
                    await GenerateEMIScheduleAsync(loan);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating loan Id {model.Id}.", ex);
            }
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

                loan.IsDeleted = true;
                loan.Active = false;
                loan.F_Updated_Date_Time = DateTime.UtcNow;

                _dbContext.Loan.Update(loan);

                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting loan Id {id}.", ex);
            }
        }

        private async Task GenerateEMIScheduleAsync(Loan loan)
        {
            var schedules = new List<LoanEMISchedule>();

            decimal loanAmount = Convert.ToDecimal(loan.LoanAmount);
            decimal annualRate = Convert.ToDecimal(loan.Rate);
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

                    // First EMI after 1 month from StartDate
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

            decimal loanAmount = Convert.ToDecimal(loan.LoanAmount);
            decimal annualRate = Convert.ToDecimal(loan.Rate);
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

            DateTime dueDate = loan.StartDate;

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
                    DueDate = dueDate,

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

                dueDate = dueDate.AddMonths(1);
            }

            await _dbContext.LoanEMISchedule.AddRangeAsync(schedules);
            await _dbContext.SaveChangesAsync();
        }
        #endregion
    }
}