using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ERPWebAppModels.Profile;
using WebApp.Model.Common;
using WebApp.Service.Profile;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized", null));
            }

            var profile = await _profileService.GetProfileAsync(userId);
            return Ok(new ApiResponse(true, "Profile fetched successfully", profile));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
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

            var result = await _profileService.UpdateProfileAsync(userId, dto);
            if (!result)
            {
                return BadRequest(new ApiResponse(false, "Could not update profile.", null));
            }

            return Ok(new ApiResponse(true, "Profile updated successfully", new { success = true }));
        }

        private string GetUserId()
        {
            return User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }
    }
}
