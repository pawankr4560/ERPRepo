using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Common;
using WebApp.Model.Menu;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly WebAppDbContext _context;

        public MenuController(WebAppDbContext context)
        {
            _context = context;
        }

        // ✅ GET ALL ROOT MENUS (TREE)
        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            var menus = await _context.MenuItem
      .Where(x => x.IsActive)
      .OrderBy(x => x.OrderNumber)
      .Select(x => new MenuTreeDto
      {
          Id = x.Id,
          Title = x.Title,
          Route = x.Route,
          IconClass = x.IconClass,
          OrderNumber = x.OrderNumber,
          ParentId = x.ParentId,
          Children = new List<MenuTreeDto>()
      })
      .ToListAsync();

            var lookup = menus.ToDictionary(x => x.Id);
            var result = new List<MenuTreeDto>();

            foreach (var menu in menus)
            {
                if (menu.ParentId == null || menu.ParentId == 0)
                {
                    result.Add(menu);
                }
                else if (lookup.ContainsKey(menu.ParentId.Value))
                {
                    lookup[menu.ParentId.Value].Children.Add(menu);
                }
            }

            return Ok(new ApiResponse(true, null, result));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllMenus()
        {
            var menus = await _context.MenuItem
                .AsNoTracking()
                .OrderBy(x => x.ParentId ?? 0)
                .ThenBy(x => x.OrderNumber)
                .ThenBy(x => x.Title)
                .Select(x => new MenuItemDto
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Title = x.Title,
                    Route = x.Route,
                    IconClass = x.IconClass,
                    OrderNumber = x.OrderNumber,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponse(true, null, menus));
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenu(int id)
        {
            var menu = await _context.MenuItem
                .Where(x => x.Id == id)
                .Select(x => new MenuItemDto
                {
                    Id = x.Id,
                    ParentId = x.ParentId,
                    Title = x.Title,
                    Route = x.Route,
                    IconClass = x.IconClass,
                    OrderNumber = x.OrderNumber,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();

            if (menu == null)
                return NotFound(new ApiResponse(false, "Menu not found", null));

            return Ok(new ApiResponse(true, null, menu));
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MenuItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid data", null));

            dto.Title = dto.Title?.Trim() ?? string.Empty;
            dto.Route = dto.Route?.Trim();
            dto.IconClass = dto.IconClass?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new ApiResponse(false, "Title is required", null));

            if (string.IsNullOrWhiteSpace(dto.IconClass))
                return BadRequest(new ApiResponse(false, "Icon class is required", null));

            if (dto.ParentId.HasValue && dto.ParentId.Value > 0)
            {
                var parentExists = await _context.MenuItem.AnyAsync(x => x.Id == dto.ParentId.Value);
                if (!parentExists)
                    return BadRequest(new ApiResponse(false, "Parent menu does not exist", null));
            }
            else
            {
                dto.ParentId = 0;
            }

            var menu = new MenuItems
            {
                ParentId = dto.ParentId,
                Title = dto.Title,
                Route = dto.Route,
                IconClass = dto.IconClass,
                OrderNumber = dto.OrderNumber,
                IsActive = dto.IsActive
            };

            _context.MenuItem.Add(menu);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse(true, "Menu created successfully", new MenuItemDto
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                Title = menu.Title,
                Route = menu.Route,
                IconClass = menu.IconClass,
                OrderNumber = menu.OrderNumber,
                IsActive = menu.IsActive
            }));
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MenuItemDto dto)
        {
            var menu = await _context.MenuItem.FindAsync(id);

            if (menu == null)
                return NotFound(new ApiResponse(false, "Menu not found", null));

            dto.Title = dto.Title?.Trim() ?? string.Empty;
            dto.Route = dto.Route?.Trim();
            dto.IconClass = dto.IconClass?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new ApiResponse(false, "Title is required", null));

            if (string.IsNullOrWhiteSpace(dto.IconClass))
                return BadRequest(new ApiResponse(false, "Icon class is required", null));

            if (dto.ParentId.HasValue && dto.ParentId.Value > 0)
            {
                if (dto.ParentId.Value == id)
                    return BadRequest(new ApiResponse(false, "Menu cannot be its own parent", null));

                var parentExists = await _context.MenuItem.AnyAsync(x => x.Id == dto.ParentId.Value);
                if (!parentExists)
                    return BadRequest(new ApiResponse(false, "Parent menu does not exist", null));
            }
            else
            {
                dto.ParentId = 0;
            }

            menu.ParentId = dto.ParentId;
            menu.Title = dto.Title;
            menu.Route = dto.Route;
            menu.IconClass = dto.IconClass;
            menu.OrderNumber = dto.OrderNumber;
            menu.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse(true, "Menu updated successfully", new MenuItemDto
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                Title = menu.Title,
                Route = menu.Route,
                IconClass = menu.IconClass,
                OrderNumber = menu.OrderNumber,
                IsActive = menu.IsActive
            }));
        }

        // ✅ DELETE (PREVENT DELETE IF CHILD EXISTS)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _context.MenuItem
                .FirstOrDefaultAsync(x => x.Id == id);

            if (menu == null)
                return NotFound(new ApiResponse(false, "Menu not found", null));

            var hasChildren = await _context.MenuItem.AnyAsync(x => x.ParentId == id);
            if (hasChildren)
                return BadRequest(new ApiResponse(false, "Delete child menus before deleting this menu", null));

            _context.MenuItem.Remove(menu);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse(true, "Menu deleted successfully", null));
        }
    }
}
