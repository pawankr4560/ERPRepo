
namespace WebApp.Service.Message
{
    public interface IMessageService
    {
        Task<bool> SendLoanSmsAsync(string mobileNumber, string loanNo, string amount);
    }
}