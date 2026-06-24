using ERPWebAppModels.Booking;
using ERPWebAppService.Booking.Car;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BookingPaymentController : ControllerBase
{
    private readonly IBookingPaymentService _service;

    public BookingPaymentController(IBookingPaymentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? bookingId)
    {
        return Ok(await _service.GetAllAsync(bookingId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var payment = await _service.GetByIdAsync(id);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BookingPaymentDto dto)
    {
        try
        {
            var payment = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = payment.Id }, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BookingPaymentDto dto)
    {
        try
        {
            var payment = await _service.UpdateAsync(id, dto);
            return payment is null ? NotFound() : Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
