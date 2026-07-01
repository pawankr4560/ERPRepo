using ERPWebAppService.Dashbord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Service.Transaction;

namespace ERPWebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ILoanDashboardService _dashboardService;
    private readonly IDashbordService _userDashbordService;

    public DashboardController(ILoanDashboardService dashboardService,IDashbordService userDashbordService)
    {
        _dashboardService = dashboardService;
        _userDashbordService =userDashbordService;
    }

    [HttpGet("loan-summary")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetLoanSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard(string userId)
    {
        var result = await _userDashbordService.GetDashboardAsync(userId);

        if (result == null)
            return NotFound("User not found");

        return Ok(result);
    }
}
