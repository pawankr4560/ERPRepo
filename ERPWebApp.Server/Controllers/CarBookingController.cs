using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ERPWebAppModels.CarBooking;
using WebApp.Model.Common;
using WebApp.Service.CarBooking;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarBookingController : ControllerBase
    {
        private readonly ICarBookingService _carBookingService;

        public CarBookingController(ICarBookingService carBookingService)
        {
            _carBookingService = carBookingService ?? throw new ArgumentNullException(nameof(carBookingService));
        }

        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            var vehicles = await _carBookingService.GetVehiclesAsync();
            return Ok(new ApiResponse(true, "Vehicles fetched successfully", vehicles));
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _carBookingService.GetBookingsAsync();
            return Ok(new ApiResponse(true, "Car bookings fetched successfully", bookings));
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateCarBookingDto dto)
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

            var result = await _carBookingService.CreateBookingAsync(userId, dto);
            return Ok(new ApiResponse(true, "Car booking created successfully", result));
        }

        private string GetUserId()
        {
            return User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }
    }
}
