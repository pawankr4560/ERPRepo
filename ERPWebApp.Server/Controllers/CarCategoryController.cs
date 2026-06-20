using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace ERPWebApp.Server.Controllers;

[Route("api/car-categories")]
[ApiController]
public class CarCategoryController : ControllerBase
{
    private readonly WebAppDbContext _context;

    public CarCategoryController(WebAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new
            {
                category.Id,
                category.Name
            })
            .ToListAsync();

        return Ok(categories);
    }
}
