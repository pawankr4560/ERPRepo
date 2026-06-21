using ERPWebAppModels.Booking;
using ERPWebAppService.Booking.Car;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Server.Controllers;

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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var booking = await _service.GetByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions()
    {
        return Ok(await _service.GetOptionsAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto dto)
    {
        try
        {
            var booking = await _service.CreateBookingAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = booking.Id }, booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateBookingDto dto)
    {
        try
        {
            var booking = await _service.UpdateBookingAsync(id, dto);
            return booking is null ? NotFound() : Ok(booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _service.DeleteBookingAsync(id) ? NoContent() : NotFound();
    }
}
