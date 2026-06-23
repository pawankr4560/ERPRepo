using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Menu;

namespace WebApp.Service.Menu;

public sealed class MenuItemService : IMenuItemService
{
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;

    public MenuItemService(MongoDbContext context, IMongoSequenceService sequences)
    {
        _context = context;
        _sequences = sequences;
    }

    public async Task<List<MenuTreeDto>> GetTreeAsync()
    {
        var menus = (await _context.MenuItems
            .Find(x => x.IsActive && !x.IsDeleted)
            .SortBy(x => x.OrderNumber)
            .ToListAsync()).Select(x => new MenuTreeDto
            {
                Id = x.Id, Title = x.Title, Route = x.Route, IconClass = x.IconClass,
                OrderNumber = x.OrderNumber, ParentId = x.ParentId, Children = []
            }).ToList();
        var lookup = menus.ToDictionary(x => x.Id);
        var roots = new List<MenuTreeDto>();
        foreach (var menu in menus)
        {
            if (menu.ParentId is null or 0) roots.Add(menu);
            else if (lookup.TryGetValue(menu.ParentId.Value, out var parent)) parent.Children.Add(menu);
        }
        return roots;
    }

    public async Task<MenuItemDto?> GetByIdAsync(int id)
    {
        var x = await _context.MenuItems
            .Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
        return x == null ? null : new MenuItemDto
        {
            ParentId = x.ParentId, Title = x.Title, Route = x.Route,
            IconClass = x.IconClass, OrderNumber = x.OrderNumber, IsActive = x.IsActive
        };
    }

    public async Task<int> CreateAsync(MenuItemDto dto)
    {
        var item = new MenuItems
        {
            Id = await _sequences.GetNextAsync("MenuItems"),
            ParentId = dto.ParentId, Title = dto.Title, Route = dto.Route,
            IconClass = dto.IconClass, OrderNumber = dto.OrderNumber,
            IsActive = dto.IsActive
        };
        await _context.MenuItems.InsertOneAsync(item);
        return item.Id;
    }

    public async Task<bool> UpdateAsync(int id, MenuItemDto dto)
    {
        var result = await _context.MenuItems.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            Builders<MenuItems>.Update
                .Set(x => x.ParentId, dto.ParentId)
                .Set(x => x.Title, dto.Title)
                .Set(x => x.Route, dto.Route)
                .Set(x => x.IconClass, dto.IconClass)
                .Set(x => x.OrderNumber, dto.OrderNumber)
                .Set(x => x.IsActive, dto.IsActive));
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (await _context.MenuItems.Find(x =>
            x.ParentId == id && !x.IsDeleted).AnyAsync())
            throw new InvalidOperationException("Menu cannot be deleted while it has child items.");
        var result = await _context.MenuItems.UpdateOneAsync(
            x => x.Id == id && !x.IsDeleted,
            Builders<MenuItems>.Update
                .Set(x => x.IsDeleted, true)
                .Set(x => x.IsActive, false));
        return result.ModifiedCount > 0;
    }
}
