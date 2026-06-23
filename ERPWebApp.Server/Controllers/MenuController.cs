using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Common;
using WebApp.Model.Menu;
using WebApp.Service.Menu;

namespace WebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuController : ControllerBase
{
    private readonly IMenuItemService _service;
    public MenuController(IMenuItemService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetMenus() =>
        Ok(new ApiResponse(true, null, await _service.GetTreeAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenu(int id)
    {
        var menu = await _service.GetByIdAsync(id);
        return menu == null
            ? NotFound(new ApiResponse(false, "Menu not found", null))
            : Ok(new ApiResponse(true, null, menu));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MenuItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse(false, "Invalid data", null));
        return Ok(new ApiResponse(true, "Menu created successfully",
            await _service.CreateAsync(dto)));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MenuItemDto dto) =>
        await _service.UpdateAsync(id, dto)
            ? Ok(new ApiResponse(true, "Menu updated successfully", null))
            : NotFound(new ApiResponse(false, "Menu not found", null));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            return await _service.DeleteAsync(id)
                ? Ok(new ApiResponse(true, "Menu deleted successfully", null))
                : NotFound(new ApiResponse(false, "Menu not found", null));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse(false, ex.Message, null));
        }
    }
}
