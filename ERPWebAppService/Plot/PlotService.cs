using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPWebAppData.Entity;
using WebApp.Data.Repository;
using ERPWebAppModels.Plot;

namespace WebApp.Service.Plot
{
    public class PlotService : IPlotService
    {
        private readonly IPlotRepository _plotRepo;

        public PlotService(IPlotRepository plotRepo)
        {
            _plotRepo = plotRepo ?? throw new ArgumentNullException(nameof(plotRepo));
        }

        public async Task<List<PlotListingDto>> GetListingsAsync()
        {
            var listings = await _plotRepo.GetAllAsync();
            return listings.Select(p => new PlotListingDto
            {
                Id = p.Id,
                PlotNumber = p.PlotNumber,
                ProjectName = p.ProjectName,
                Location = p.Location,
                AreaSqFt = p.AreaSqFt,
                Price = p.Price,
                Status = p.Status,
                Facing = p.Facing
            }).ToList();
        }

        public async Task<PlotListingDto> CreateListingAsync(PlotListingDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var plot = new PlotListing
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                PlotNumber = dto.PlotNumber,
                ProjectName = dto.ProjectName,
                Location = dto.Location,
                AreaSqFt = dto.AreaSqFt,
                Price = dto.Price,
                Status = dto.Status,
                Facing = dto.Facing,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _plotRepo.InsertAsync(plot);
            await _plotRepo.SaveAsync();

            return new PlotListingDto
            {
                Id = plot.Id,
                PlotNumber = plot.PlotNumber,
                ProjectName = plot.ProjectName,
                Location = plot.Location,
                AreaSqFt = plot.AreaSqFt,
                Price = plot.Price,
                Status = plot.Status,
                Facing = plot.Facing
            };
        }
    }
}
