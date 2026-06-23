using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebApp.Data;

namespace ERPWebApp.Server.Controllers;

[Route("api/car-categories")]
[ApiController]
public class CarCategoryController : ControllerBase
{
    private readonly MongoDbContext _context;
    public CarCategoryController(MongoDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var categories = await _context.Categories
            .Find(Builders<ERPWebAppData.Entity.Category>.Filter.Empty)
            .SortBy(x => x.Name)
            .Project(x => new { x.Id, x.Name })
            .ToListAsync();
        return Ok(categories);
    }
}
