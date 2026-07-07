using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public class PlotRepository : GenericRepository<PlotListing>, IPlotRepository
    {
        public PlotRepository(WebAppDbContext context) : base(context)
        {
        }
    }
}
