using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction
{
    public class LoanEMIScheduleService : ILoanEMIScheduleService
    {
        private readonly WebAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public LoanEMIScheduleService(
            WebAppDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<LoanEMIScheduleDto>> GetAllAsync()
        {
            var schedules = await _dbContext.LoanEMISchedule
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.InstallmentNo)
                .ToListAsync();

            return _mapper.Map<List<LoanEMIScheduleDto>>(schedules);
        }

        public async Task<LoanEMIScheduleDto?> GetByIdAsync(int id)
        {
            var schedule = await _dbContext.LoanEMISchedule
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            return schedule == null
                ? null
                : _mapper.Map<LoanEMIScheduleDto>(schedule);
        }
        public async Task<List<LoanEMIScheduleDto>> GetScheduleByLoanNumberAsync(string loanNumber)
        {
            var schedules = await (
                from loan in _dbContext.Loan
                join schedule in _dbContext.LoanEMISchedule
                    on loan.Id equals schedule.LoanId
                join add in _dbContext.LoanCustomerDetail
                on loan.Id equals add.LoanId
                where loan.LoanNumber == loanNumber
                      && !loan.IsDeleted
                      && !schedule.IsDeleted
                orderby schedule.InstallmentNo
                select new LoanEMIScheduleDto
                {
                    Id = schedule.Id,
                    LoanId = schedule.LoanId,
                    InstallmentNo = schedule.InstallmentNo,
                    DueDate = schedule.DueDate,
                    EMIAmount = schedule.EMIAmount,
                    PrincipalAmount = schedule.PrincipalAmount,
                    InterestAmount = schedule.InterestAmount,
                    OutstandingBalance = schedule.OutstandingBalance,
                    IsPaid = schedule.IsPaid,
                    CustMobNo = add.CustomerMobileNo,
                    GranterMobNo= add.CustomerMobileNo,
                    GranterName=add.GuarantorName,
                    Relation=add.GuarantorRelationship,
                    PaidDate = schedule.PaidDate
                }
            ).ToListAsync();

            return schedules;
        }
        public async Task<List<LoanEMIScheduleDto>> GetByLoanIdAsync(int loanId)
        {
            var schedules = await _dbContext.LoanEMISchedule
                .Where(x =>
                    x.LoanId == loanId &&
                    !x.IsDeleted)
                .OrderBy(x => x.InstallmentNo)
                .ToListAsync();

            return _mapper.Map<List<LoanEMIScheduleDto>>(schedules);
        }

        public async Task<LoanEMIScheduleDto> AddAsync(LoanEMIScheduleDto model)
        {
            var entity = _mapper.Map<LoanEMISchedule>(model);

            entity.Active = true;
            entity.IsDeleted = false;
            entity.F_Created_Date_Time = DateTime.UtcNow;

            await _dbContext.LoanEMISchedule.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<LoanEMIScheduleDto>(entity);
        }

        public async Task<List<LoanInstallmentDto>> GetUnpaidInstallmentsByLoanNumber(string loanNumber)
        {
            var result = await (
                from loan in _dbContext.Loan
                join schedule in _dbContext.LoanEMISchedule
                    on loan.Id equals schedule.LoanId
                where loan.LoanNumber == loanNumber
                      && !loan.IsDeleted
                      && !schedule.IsDeleted
                      && !schedule.IsPaid
                orderby schedule.InstallmentNo
                select new LoanInstallmentDto
                {
                    LoanId = loan.Id,
                    ScheduleId = schedule.Id,
                    InstallmentNo = schedule.InstallmentNo,
                    DueDate = schedule.DueDate,
                    EMIAmount = schedule.EMIAmount,
                    PrincipalAmount = schedule.PrincipalAmount,
                    InterestAmount = schedule.InterestAmount,
                    OutstandingBalance = schedule.OutstandingBalance
                }
            ).ToListAsync();

            return result;
        }

        public async Task<bool> UpdateAsync(int id, LoanEMIScheduleDto model)
        {
            var entity = await _dbContext.LoanEMISchedule
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (entity == null)
                return false;

            entity.LoanId = model.LoanId;
            entity.InstallmentNo = model.InstallmentNo;
            entity.DueDate = model.DueDate;
            entity.EMIAmount = model.EMIAmount;
            entity.PrincipalAmount = model.PrincipalAmount;
            entity.InterestAmount = model.InterestAmount;
            entity.OutstandingBalance = model.OutstandingBalance;
            entity.IsPaid = model.IsPaid;
            entity.PaidDate = model.PaidDate;
            entity.F_Updated_Date_Time = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.LoanEMISchedule
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.Active = false;
            entity.F_Updated_Date_Time = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
