using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction
{
    public class LoanPaymentService : ILoanPaymentService
    {
        private readonly WebAppDbContext _dbContext;
        private readonly IMapper _mapper;

        public LoanPaymentService(
            WebAppDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<LoanPaymentDto>> GetAllAsync()
        {
            var payments = await _dbContext.LoanPayment
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();

            return _mapper.Map<List<LoanPaymentDto>>(payments);
        }

        public async Task<LoanPaymentDto?> GetByIdAsync(int id)
        {
            var payment = await _dbContext.LoanPayment
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            return payment == null
                ? null
                : _mapper.Map<LoanPaymentDto>(payment);
        }

        public async Task<LoanPaymentDto> AddAsync(LoanPaymentDto model)
        {
            var entity = _mapper.Map<LoanPayment>(model);

            entity.Active = true;
            entity.IsDeleted = false;
            entity.F_Created_Date_Time = DateTime.UtcNow;

            await _dbContext.LoanPayment.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<LoanPaymentDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, LoanPaymentDto model)
        {
            try
            {
                var entity = await _dbContext.LoanPayment
                    .FirstOrDefaultAsync(x =>
                        x.Id == id &&
                        !x.IsDeleted);

                if (entity == null)
                    return false;

                entity.LoanId = model.LoanId;
                entity.PaymentMode = model.PaymentMode;
                entity.Remarks = model.Remarks;
                entity.F_Updated_Date_Time = DateTime.UtcNow;

                return await _dbContext.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating loan payment Id {id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.LoanPayment
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
