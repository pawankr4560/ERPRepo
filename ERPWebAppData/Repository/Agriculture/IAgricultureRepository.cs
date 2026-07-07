using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public interface IAgricultureRepository
    {
        Task<List<FieldRecord>> GetFieldsAsync();
        Task<List<AgricultureStockItem>> GetStockAsync();
        Task<FieldRecord?> GetFieldByIdAsync(Guid fieldId);
        Task InsertFieldAsync(FieldRecord field);
        Task InsertStockAsync(AgricultureStockItem stock);
        Task UpdateFieldAsync(FieldRecord field);
        Task SaveAsync();
    }
}
