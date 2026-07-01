using ERPWebAppModels.Dashbord;

namespace ERPWebAppService.Dashbord
{
    public interface IDashbordService
    {
        Task<DashboardDto?> GetDashboardAsync(string userId);
    }
}