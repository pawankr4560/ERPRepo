using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPWebAppData.Entity;
using WebApp.Data.Repository;
using ERPWebAppModels.Inventory;
using WebApp.Service.Dashboard;

namespace WebApp.Service.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IDashboardService _dashboardService;

        public InventoryService(IInventoryRepository inventoryRepo, IDashboardService dashboardService)
        {
            _inventoryRepo = inventoryRepo ?? throw new ArgumentNullException(nameof(inventoryRepo));
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        public async Task<List<InventoryItemDto>> GetItemsAsync()
        {
            var items = await _inventoryRepo.GetAllAsync();
            return items.Select(i => new InventoryItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Category = i.Category,
                CurrentStock = i.CurrentStock,
                LowStockThreshold = i.LowStockThreshold,
                CostPrice = i.CostPrice,
                SellingPrice = i.SellingPrice
            }).ToList();
        }

        public async Task<InventoryItemDto> CreateItemAsync(string userId, CreateInventoryItemDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var item = new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Category = dto.Category,
                CurrentStock = dto.CurrentStock,
                LowStockThreshold = dto.LowStockThreshold,
                CostPrice = dto.CostPrice,
                SellingPrice = dto.SellingPrice,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _inventoryRepo.InsertAsync(item);
            await _inventoryRepo.SaveAsync();

            await _dashboardService.LogActivityAsync(
                userId,
                "Inventory Item Added",
                $"Item: {dto.Name}, Category: {dto.Category}, Stock: {dto.CurrentStock}",
                "inventory",
                "#FF9800"
            );

            return new InventoryItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Category = item.Category,
                CurrentStock = item.CurrentStock,
                LowStockThreshold = item.LowStockThreshold,
                CostPrice = item.CostPrice,
                SellingPrice = item.SellingPrice
            };
        }

        public async Task<bool> UpdateStockAsync(string userId, Guid id, int newStock)
        {
            var item = await _inventoryRepo.GetByIdAsync(id);
            if (item == null) return false;

            int oldStock = item.CurrentStock;
            item.CurrentStock = newStock;
            await _inventoryRepo.UpdateAsync(item);
            await _inventoryRepo.SaveAsync();

            await _dashboardService.LogActivityAsync(
                userId,
                "Stock Quantity Updated",
                $"Item: {item.Name}, Old Stock: {oldStock}, New Stock: {newStock}",
                "edit",
                "#2196F3"
            );

            return true;
        }
    }
}
