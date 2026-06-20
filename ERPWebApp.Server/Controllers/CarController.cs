using ERPWebAppModels.Booking;
using ERPWebAppService.Booking.Car;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CarController : ControllerBase
{
    private readonly ICarService _service;

    public CarController(ICarService service)
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
        var car = await _service.GetByIdAsync(id);
        return car is null ? NotFound() : Ok(car);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCarDto dto)
    {
        try
        {
            var car = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = car.Id }, car);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateCarDto dto)
    {
        try
        {
            var car = await _service.UpdateAsync(id, dto);
            return car is null ? NotFound() : Ok(car);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
