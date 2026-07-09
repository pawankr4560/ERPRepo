using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

namespace ERPWebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CategoryController : ControllerBase
{
    private readonly WebAppDbContext _context;

    public CategoryController(WebAppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new
            {
                id = category.Id,
                name = category.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("SubCategories")]
    public async Task<IActionResult> GetSubCategories()
    {
        var subCategories = await _context.SubCategory
            .AsNoTracking()
            .OrderBy(subCategory => subCategory.Name)
            .Select(subCategory => new
            {
                id = subCategory.Id,
                name = subCategory.Name,
                description = subCategory.Description,
                categoryId = subCategory.CategoryId
            })
            .ToListAsync();

        return Ok(subCategories);
    }
}
