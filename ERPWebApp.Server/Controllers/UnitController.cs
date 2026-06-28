using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace ERPWebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UnitController : ControllerBase
{
    private readonly WebAppDbContext _context;

    public UnitController(WebAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var units = await _context.UnitOfMeasure
            .AsNoTracking()
            .OrderBy(unit => unit.UOMName)
            .Select(unit => new
            {
                id = unit.UOMIndex,
                code = unit.UOMCode,
                name = unit.UOMName
            })
            .ToListAsync();

        return Ok(units);
    }
}
