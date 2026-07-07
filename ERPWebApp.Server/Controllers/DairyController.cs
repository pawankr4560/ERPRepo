using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ERPWebAppModels.Dairy;
using WebApp.Model.Common;
using WebApp.Service.Dairy;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DairyController : ControllerBase
    {
        private readonly IDairyService _dairyService;

        public DairyController(IDairyService dairyService)
        {
            _dairyService = dairyService ?? throw new ArgumentNullException(nameof(dairyService));
        }

        [HttpGet("collection-logs")]
        public async Task<IActionResult> GetCollectionLogs()
        {
            var logs = await _dairyService.GetCollectionLogsAsync();
            return Ok(new ApiResponse(true, "Milk collection logs fetched successfully", logs));
        }

        [HttpPost("collection-logs")]
        public async Task<IActionResult> CreateCollectionLog([FromBody] CreateMilkCollectionLogDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiResponse(false, "Request body cannot be null.", null));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Invalid request data.", ModelState));
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized", null));
            }

            var result = await _dairyService.CreateCollectionLogAsync(userId, dto);
            return Ok(new ApiResponse(true, "Milk collection log created successfully", result));
        }

        private string GetUserId()
        {
            return User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }
    }
}
