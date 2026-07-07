using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ERPWebAppModels.Inventory;
using WebApp.Model.Common;
using WebApp.Service.Inventory;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetItems()
        {
            var items = await _inventoryService.GetItemsAsync();
            return Ok(new ApiResponse(true, "Inventory items fetched successfully", items));
        }

        [HttpPost("items")]
        public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiResponse(false, "Request body cannot be null.", null));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Invalid request data.", ModelState));
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized", null));
            }

            var result = await _inventoryService.CreateItemAsync(userId, dto);
            return Ok(new ApiResponse(true, "Inventory item created successfully", result));
        }

        [HttpPatch("items/{id:guid}/stock")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiResponse(false, "Request body cannot be null.", null));
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(false, "Unauthorized", null));
            }

            var result = await _inventoryService.UpdateStockAsync(userId, id, dto.NewStock);
            if (!result)
            {
                return NotFound(new ApiResponse(false, "Inventory item not found.", null));
            }

            return Ok(new ApiResponse(true, "Inventory stock updated successfully", new { success = true }));
        }

        private string GetUserId()
        {
            return User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }
    }
}
