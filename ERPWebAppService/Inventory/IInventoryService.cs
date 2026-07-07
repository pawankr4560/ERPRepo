using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppModels.Inventory;

namespace WebApp.Service.Inventory
{
    public interface IInventoryService
    {
        Task<List<InventoryItemDto>> GetItemsAsync();
        Task<InventoryItemDto> CreateItemAsync(string userId, CreateInventoryItemDto dto);
        Task<bool> UpdateStockAsync(string userId, Guid id, int newStock);
    }
}
