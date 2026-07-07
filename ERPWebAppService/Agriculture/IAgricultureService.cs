using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppModels.Agriculture;

namespace WebApp.Service.Agriculture
{
    public interface IAgricultureService
    {
        Task<List<FieldRecordDto>> GetFieldsAsync();
        Task<List<AgricultureStockItemDto>> GetStockAsync();
        Task<FieldRecordDto?> SprayFieldAsync(string userId, Guid fieldId, DateTime applicationDate);
        Task<FieldRecordDto> CreateFieldAsync(string userId, FieldRecordDto dto);
        Task<AgricultureStockItemDto> CreateStockItemAsync(string userId, AgricultureStockItemDto dto);
    }
}
