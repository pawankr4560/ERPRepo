using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Model.Common;
using WebApp.Service.Dashboard;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized", null));
            }

            var summary = await _dashboardService.GetSummaryAsync(userId);
            return Ok(new ApiResponse(true, "Dashboard summary fetched successfully", summary));
        }

        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 10)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized", null));
            }

            var activity = await _dashboardService.GetRecentActivityAsync(userId, limit);
            return Ok(new ApiResponse(true, "Recent activity fetched successfully", activity));
        }

        private string GetUserId()
        {
            return User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }
    }
}
