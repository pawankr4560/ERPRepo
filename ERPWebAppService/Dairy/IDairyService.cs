using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppModels.Dairy;

namespace WebApp.Service.Dairy
{
    public interface IDairyService
    {
        Task<List<MilkCollectionLogDto>> GetCollectionLogsAsync();
        Task<MilkCollectionLogDto> CreateCollectionLogAsync(string userId, CreateMilkCollectionLogDto dto);
    }
}
