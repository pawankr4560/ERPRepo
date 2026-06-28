using WebApp.Model.Order;

namespace WebApp.Service.Order
{
    public interface IOrderService
    {
        Task<bool> CreateOrder(List<CreateOrderRequestModel> model);
        Task<dynamic> GetOrders();
        Task<dynamic> GetSalesItemsAsync();
        Task<SalesOrderRazorpayOrderResponse> CreateSalesOrderCheckoutAsync(SalesOrderCheckoutRequest request, string userId);
        Task<SalesOrderVerifyResponse> VerifySalesOrderPaymentAsync(SalesOrderVerifyRequest request, string userId);
    }
}
