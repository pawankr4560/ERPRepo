using ERPWebAppModels.Construction;

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
    }
}