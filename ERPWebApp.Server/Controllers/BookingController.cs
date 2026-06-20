using ERPWebAppModels.Booking;
using ERPWebAppService.Booking.Car;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingController(IBookingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingDto dto)
        {
            return Ok(await _service.CreateBookingAsync(dto));
        }
    }
}
