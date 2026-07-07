using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public class InventoryRepository : GenericRepository<InventoryItem>, IInventoryRepository
    {
        public InventoryRepository(WebAppDbContext context) : base(context)
        {
        }
    }
}
