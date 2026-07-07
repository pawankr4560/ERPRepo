using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPWebAppData.Entity;
using WebApp.Data.Repository;
using ERPWebAppModels.Agriculture;
using WebApp.Service.Dashboard;

namespace WebApp.Service.Agriculture
{
    public class AgricultureService : IAgricultureService
    {
        private readonly IAgricultureRepository _agricultureRepo;
        private readonly IDashboardService _dashboardService;

        public AgricultureService(IAgricultureRepository agricultureRepo, IDashboardService dashboardService)
        {
            _agricultureRepo = agricultureRepo ?? throw new ArgumentNullException(nameof(agricultureRepo));
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        public async Task<List<FieldRecordDto>> GetFieldsAsync()
        {
            var fields = await _agricultureRepo.GetFieldsAsync();
            return fields.Select(f => new FieldRecordDto
            {
                Id = f.Id,
                Name = f.Name,
                Crop = f.Crop,
                AreaAcres = f.AreaAcres,
                Status = f.Status,
                LastSprayedDate = f.LastSprayedDate
            }).ToList();
        }

        public async Task<List<AgricultureStockItemDto>> GetStockAsync()
        {
            var stock = await _agricultureRepo.GetStockAsync();
            return stock.Select(s => new AgricultureStockItemDto
            {
                Id = s.Id,
                Name = s.Name,
                QuantityLabel = s.QuantityLabel,
                Status = s.Status
            }).ToList();
        }

        public async Task<FieldRecordDto?> SprayFieldAsync(string userId, Guid fieldId, DateTime applicationDate)
        {
            var field = await _agricultureRepo.GetFieldByIdAsync(fieldId);
            if (field == null) return null;

            field.Status = "Healthy";
            field.LastSprayedDate = applicationDate;

            await _agricultureRepo.UpdateFieldAsync(field);
            await _agricultureRepo.SaveAsync();

            await _dashboardService.LogActivityAsync(
                userId,
                "Field Sprayed",
                $"Field: {field.Name}, Crop: {field.Crop}, Date: {applicationDate:yyyy-MM-dd}",
                "nature",
                "#8BC34A"
            );

            return new FieldRecordDto
            {
                Id = field.Id,
                Name = field.Name,
                Crop = field.Crop,
                AreaAcres = field.AreaAcres,
                Status = field.Status,
                LastSprayedDate = field.LastSprayedDate
            };
        }

        public async Task<FieldRecordDto> CreateFieldAsync(string userId, FieldRecordDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var field = new FieldRecord
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Name = dto.Name,
                Crop = dto.Crop,
                AreaAcres = dto.AreaAcres,
                Status = dto.Status,
                LastSprayedDate = dto.LastSprayedDate,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _agricultureRepo.InsertFieldAsync(field);
            await _agricultureRepo.SaveAsync();

            await _dashboardService.LogActivityAsync(
                userId,
                "Field Registered",
                $"Field: {field.Name}, Crop: {field.Crop}",
                "landscape",
                "#4CAF50"
            );

            return new FieldRecordDto
            {
                Id = field.Id,
                Name = field.Name,
                Crop = field.Crop,
                AreaAcres = field.AreaAcres,
                Status = field.Status,
                LastSprayedDate = field.LastSprayedDate
            };
        }

        public async Task<AgricultureStockItemDto> CreateStockItemAsync(string userId, AgricultureStockItemDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var item = new AgricultureStockItem
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Name = dto.Name,
                QuantityLabel = dto.QuantityLabel,
                Status = dto.Status,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _agricultureRepo.InsertStockAsync(item);
            await _agricultureRepo.SaveAsync();

            return new AgricultureStockItemDto
            {
                Id = item.Id,
                Name = item.Name,
                QuantityLabel = item.QuantityLabel,
                Status = item.Status
            };
        }
    }
}
