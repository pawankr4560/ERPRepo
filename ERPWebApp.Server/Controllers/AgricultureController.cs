using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ERPWebAppModels.Agriculture;
using WebApp.Model.Common;
using WebApp.Service.Agriculture;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AgricultureController : ControllerBase
    {
        private readonly IAgricultureService _agricultureService;

        public AgricultureController(IAgricultureService agricultureService)
        {
            _agricultureService = agricultureService ?? throw new ArgumentNullException(nameof(agricultureService));
        }

        [HttpGet("fields")]
        public async Task<IActionResult> GetFields()
        {
            var fields = await _agricultureService.GetFieldsAsync();
            return Ok(new ApiResponse(true, "Agriculture fields fetched successfully", fields));
        }

        [HttpGet("stock")]
        public async Task<IActionResult> GetStock()
        {
            var stock = await _agricultureService.GetStockAsync();
            return Ok(new ApiResponse(true, "Agriculture stock fetched successfully", stock));
        }

        [HttpPost("fields/{fieldId:guid}/spray-log")]
        public async Task<IActionResult> SprayLog(Guid fieldId, [FromBody] SprayLogDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiResponse(false, "Request body cannot be null.", null));
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized", null));
            }

            var updatedField = await _agricultureService.SprayFieldAsync(userId, fieldId, dto.ApplicationDate);
            if (updatedField == null)
            {
                return NotFound(new ApiResponse(false, "Field record not found.", null));
            }

            return Ok(new ApiResponse(true, "Spray log saved successfully", updatedField));
        }

        private string GetUserId()
        {
            return User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }
    }
}
