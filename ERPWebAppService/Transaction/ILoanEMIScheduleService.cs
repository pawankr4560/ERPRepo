using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction
{
    public interface ILoanEMIScheduleService
    {
        Task<LoanEMIScheduleDto> AddAsync(LoanEMIScheduleDto model);
        Task<bool> DeleteAsync(int id);
        Task<List<LoanEMIScheduleDto>> GetScheduleByLoanNumberAsync(string loanNumber);
        Task<List<LoanInstallmentDto>> GetUnpaidInstallmentsByLoanNumber(string loanNumber);
        Task<List<LoanEMIScheduleDto>> GetAllAsync();
        Task<LoanEMIScheduleDto?> GetByIdAsync(int id);
        Task<List<LoanEMIScheduleDto>> GetByLoanIdAsync(int loanId);
        Task<bool> UpdateAsync(int id, LoanEMIScheduleDto model);
    }
}