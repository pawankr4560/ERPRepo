using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppModels.Dashboard;

namespace WebApp.Service.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(string userId);
        Task<List<RecentActivityDto>> GetRecentActivityAsync(string userId, int count = 10);
        Task LogActivityAsync(string userId, string title, string subtitle, string iconName, string hexColor);
    }
}
