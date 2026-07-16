using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public class PlotRepository : GenericRepository<Plot>, IPlotRepository
    {
        public PlotRepository(WebAppDbContext context) : base(context)
        {
        }
    }
}
