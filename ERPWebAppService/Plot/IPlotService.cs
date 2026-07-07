using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppModels.Plot;

namespace WebApp.Service.Plot
{
    public interface IPlotService
    {
        Task<List<PlotListingDto>> GetListingsAsync();
        Task<PlotListingDto> CreateListingAsync(PlotListingDto dto);
    }
}
