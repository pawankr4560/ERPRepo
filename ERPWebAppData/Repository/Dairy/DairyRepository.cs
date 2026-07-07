using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public class DairyRepository : GenericRepository<MilkCollectionLog>, IDairyRepository
    {
        public DairyRepository(WebAppDbContext context) : base(context)
        {
        }
    }
}
