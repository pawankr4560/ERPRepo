using ERPWebAppModels.Transaction;
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
        Task<bool> UpdateLoan(LoanUpdateRequestModel model);
        Task<bool> ApproveLoan(int id, string approvedByUserId);
        Task<bool> RejectLoan(int id, string rejectedByUserId);
        Task<CreateLoanApplicationResponseDto> CreateLoanApplication(
    CreateLoanApplicationRequestDto model);
        Task<List<LoanApplicationListDto>> GetLoanApplicationsAsync(string userId);
        Task<LoanApplicationStatusDto> GetLoanApplicationStatusAsync(
    string userId,
    string applicationId);
        Task<UploadLoanDocumentsResponseDto> UploadLoanDocumentsAsync(
    UploadLoanDocumentsRequestDto request);
    }
}
