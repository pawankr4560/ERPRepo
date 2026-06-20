using WebApp.Model.Order;

namespace WebApp.Service.Order
{
    public interface IOrderService
    {
        Task<bool> CreateOrder(List<CreateOrderRequestModel> model);
        Task<dynamic> GetOrders();
    }
}