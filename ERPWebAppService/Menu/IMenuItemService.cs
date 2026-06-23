using WebApp.Model.Menu;

namespace WebApp.Service.Menu;

public interface IMenuItemService
{
    Task<List<MenuTreeDto>> GetTreeAsync();
    Task<MenuItemDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(MenuItemDto dto);
    Task<bool> UpdateAsync(int id, MenuItemDto dto);
    Task<bool> DeleteAsync(int id);
}
