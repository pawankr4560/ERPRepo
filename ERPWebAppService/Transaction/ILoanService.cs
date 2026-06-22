using WebApp.Model.Transaction;

namespace WebApp.Service.Transaction
{
    public interface ILoanService
    {
        Task<bool> AddLoan(LoanRequestModel model);
        Task<bool> DeleteLoan(int id);
        Task<CreateLoanDTO> GetLoanNumber();
        Task<LoanDto?> GetLoanById(int id);
        Task<List<LoanDto>> LoanList();
        Task<bool> UpdateLoan(LoanRequestModel model);
        Task<bool> ApproveLoan(int id, string approvedByUserId);
        Task<bool> RejectLoan(int id, string rejectedByUserId);
    }
}
