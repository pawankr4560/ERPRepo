using ERPWebAppModels.Construction;
using ERPWebAppModels.Menu;

namespace ERPWebAppService.Construction
{
    public interface IConstructionService
    {
        Task<ConstructionDashboardDto> DashbordData(string userId);
        Task<ConstructionProductListDto> GetProducts(
        int? categoryId,
        string? search,
        int page = 1,
        int limit = 20);
        Task<ConstructionQuoteResponseDto> CreateQuote(
    ConstructionQuoteRequest request,string userId);
        Task<List<CategorieDto>> GetCategories();
        Task<List<UnitDto>> Units();
        Task<List<ConstructionQuoteDto>> GetQuotes(string userId, bool isAdmin = false);
        Task<UpdateQuotePriceResponseDto> UpdateQuotePrice(
    int quoteId,
    UpdateQuotePriceRequest request);
        Task<CreateConstructionOrderResponseDto> CreateOrderFromQuote(int quoteId, string userId);
        Task<List<ConstructionOrderDto>> GetOrders(string userId, bool isAdmin = false);
        Task<List<ConstructionDeliveryDto>> GetDeliveries(string userId, bool isAdmin = false);
        Task<UpdateConstructionDeliveryResponseDto> CreateDeliveryForOrder(int orderId);
        Task<UpdateConstructionOrderStatusResponseDto> UpdateOrderStatus(
    int orderId,
    UpdateConstructionOrderStatusRequest request);
        Task<UpdateConstructionDeliveryResponseDto> UpdateDelivery(
    int deliveryId,
    UpdateConstructionDeliveryRequest request);
    }
}
