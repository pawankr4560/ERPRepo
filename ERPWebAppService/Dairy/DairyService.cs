using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPWebAppData.Entity;
using WebApp.Data.Repository;
using ERPWebAppModels.Dairy;
using WebApp.Service.Dashboard;

namespace WebApp.Service.Dairy
{
    public class DairyService : IDairyService
    {
        private readonly IDairyRepository _dairyRepo;
        private readonly IDashboardService _dashboardService;

        public DairyService(IDairyRepository dairyRepo, IDashboardService dashboardService)
        {
            _dairyRepo = dairyRepo ?? throw new ArgumentNullException(nameof(dairyRepo));
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        public async Task<List<MilkCollectionLogDto>> GetCollectionLogsAsync()
        {
            var logs = await _dairyRepo.GetAllAsync();
            return logs.Select(l => new MilkCollectionLogDto
            {
                Id = l.Id,
                FarmerName = l.FarmerName,
                Shift = l.Shift,
                QuantityInLiters = l.QuantityInLiters,
                FatPercentage = l.FatPercentage,
                RatePerLiter = l.RatePerLiter,
                Date = l.Date
            }).ToList();
        }

        public async Task<MilkCollectionLogDto> CreateCollectionLogAsync(string userId, CreateMilkCollectionLogDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var log = new MilkCollectionLog
            {
                Id = Guid.NewGuid(),
                FarmerName = dto.FarmerName,
                Shift = dto.Shift,
                QuantityInLiters = dto.QuantityInLiters,
                FatPercentage = dto.FatPercentage,
                RatePerLiter = dto.RatePerLiter,
                Date = dto.Date,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _dairyRepo.InsertAsync(log);
            await _dairyRepo.SaveAsync();

            await _dashboardService.LogActivityAsync(
                userId,
                "Milk Collection Logged",
                $"Farmer: {dto.FarmerName}, Qty: {dto.QuantityInLiters}L, Rate: {dto.RatePerLiter}/L",
                "local_shipping",
                "#4CAF50"
            );

            return new MilkCollectionLogDto
            {
                Id = log.Id,
                FarmerName = log.FarmerName,
                Shift = log.Shift,
                QuantityInLiters = log.QuantityInLiters,
                FatPercentage = log.FatPercentage,
                RatePerLiter = log.RatePerLiter,
                Date = log.Date
            };
        }
    }
}
