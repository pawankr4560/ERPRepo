using ERPWebAppModels.Plot;

namespace WebApp.Service.Plot
{
    public interface IPlotService
    {
        Task<PlotDetailsDto?> GetPlotByIdAsync(Guid plotId, string userId);
        Task<PlotListResponseDto> GetPlotsAsync(string? search, string? status, string? location, string? propertyType, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea, int page = 1, int pageSize = 20);
        Task<List<SavedPlotDto>> GetSavedPlotsAsync(string userId);
        Task<bool> RemoveSavedPlotAsync(Guid plotId, string userId);
        Task<SavePlotResponseDto> SavePlotAsync(Guid plotId, string userId);
        Task<PlotVisitResponseDto> BookSiteVisitAsync(
        Guid plotId,
        string userId,
        BookSiteVisitRequest request);

        Task<PlotVisitListResponseDto> GetMyVisitsAsync(
        string userId,
        string? status,
        int page = 1,
        int pageSize = 20);
        Task<List<AdminPlotDto>> GetAdminPlotsAsync(string? search);
        Task<AdminPlotDto> CreatePlotAsync(AdminPlotRequest request);
        Task<AdminPlotDto?> UpdatePlotAsync(Guid plotId, AdminPlotRequest request);
        Task<bool> DeletePlotAsync(Guid plotId);
        Task<List<AdminAmenityDto>> GetAmenitiesAsync();
        Task<List<AdminPlotVisitDto>> GetAdminVisitsAsync(string? status, string? search = null);
        Task<AdminPlotVisitDto?> UpdateVisitStatusAsync(Guid visitId, string status);
    }
}
