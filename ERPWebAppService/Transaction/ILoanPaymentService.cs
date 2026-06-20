
using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction
{
    public interface ILoanPaymentService
    {
        Task<LoanPaymentDto> AddAsync(LoanPaymentDto model);
        Task<bool> DeleteAsync(int id);
        Task<List<LoanPaymentDto>> GetAllAsync();
        Task<LoanPaymentDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, LoanPaymentDto model);
    }
}