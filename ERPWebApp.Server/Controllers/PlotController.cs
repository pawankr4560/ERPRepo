using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApp.Model.Common;
using WebApp.Service.Plot;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlotController : ControllerBase
    {
        private readonly IPlotService _plotService;

        public PlotController(IPlotService plotService)
        {
            _plotService = plotService ?? throw new ArgumentNullException(nameof(plotService));
        }

        [HttpGet("listings")]
        public async Task<IActionResult> GetListings()
        {
            var listings = await _plotService.GetListingsAsync();
            return Ok(new ApiResponse(true, "Plot listings fetched successfully", listings));
        }
    }
}
