using Microsoft.AspNetCore.Mvc;
using WebApp.Service.Transaction;

namespace ERPWebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly ILoanDashboardService _dashboardService;

    public DashboardController(ILoanDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("loan-summary")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetLoanSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(result);
    }
}
