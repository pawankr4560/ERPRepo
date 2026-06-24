using Microsoft.AspNetCore.Mvc;
using ERPWebAppService.Booking.Car;

namespace ERPWebApp.Server.Controllers;

[Route("api/car-categories")]
[ApiController]
public class CarCategoryController : ControllerBase
{
    private readonly ICarService _carService;

    public CarCategoryController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var categories = await _carService.GetCategoriesAsync();

        return Ok(categories
            .Select(category => new
            {
                category.Id,
                category.Name
            }));
    }
}
