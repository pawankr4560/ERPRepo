using ERPWebAppModels.Payment;

namespace ERPWebAppService.Payment
{
    public interface IPaymentService
    {
        Task<UserPaymentsResponseDto> GetUserPaymentsAsync(string userId);
    }
}