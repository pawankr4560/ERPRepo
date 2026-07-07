using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public class AgricultureRepository : IAgricultureRepository
    {
        private readonly WebAppDbContext _context;

        public AgricultureRepository(WebAppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<FieldRecord>> GetFieldsAsync()
        {
            return await _context.FieldRecords.ToListAsync();
        }

        public async Task<List<AgricultureStockItem>> GetStockAsync()
        {
            return await _context.AgricultureStockItems.ToListAsync();
        }

        public async Task<FieldRecord?> GetFieldByIdAsync(Guid fieldId)
        {
            return await _context.FieldRecords.FindAsync(fieldId);
        }

        public async Task InsertFieldAsync(FieldRecord field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            await _context.FieldRecords.AddAsync(field);
        }

        public async Task InsertStockAsync(AgricultureStockItem stock)
        {
            if (stock == null) throw new ArgumentNullException(nameof(stock));
            await _context.AgricultureStockItems.AddAsync(stock);
        }

        public Task UpdateFieldAsync(FieldRecord field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            _context.FieldRecords.Attach(field);
            _context.Entry(field).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
