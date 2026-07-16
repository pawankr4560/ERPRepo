using ERPWebAppModels.Plot;
using WebApp.Data;
using Microsoft.EntityFrameworkCore;
using ERPWebAppData.Entity;

namespace WebApp.Service.Plot
{
    public class PlotService : IPlotService
    {
        private readonly WebAppDbContext dbContext;

        public PlotService(WebAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PlotListResponseDto> GetPlotsAsync(
        string? search,
        string? status,
        string? location,
        string? propertyType,
        decimal? minPrice,
        decimal? maxPrice,
        decimal? minArea,
        decimal? maxArea,
        int page = 1,
        int pageSize = 20)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = dbContext.Plots
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim();

                query = query.Where(x =>
                    x.Title.Contains(searchText) ||
                    x.Location.Contains(searchText) ||
                    x.PropertyType.Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusText = status.Trim();

                query = query.Where(x => x.Status == statusText);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var locationText = location.Trim();

                query = query.Where(x =>
                    x.Location.Contains(locationText));
            }

            if (!string.IsNullOrWhiteSpace(propertyType))
            {
                var propertyTypeText = propertyType.Trim();

                query = query.Where(x =>
                    x.PropertyType == propertyTypeText);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x =>
                    x.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x =>
                    x.Price <= maxPrice.Value);
            }

            if (minArea.HasValue)
            {
                query = query.Where(x =>
                    x.AreaSqFt >= minArea.Value);
            }

            if (maxArea.HasValue)
            {
                query = query.Where(x =>
                    x.AreaSqFt <= maxArea.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.AreaSqFt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PlotListingDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Location = x.Location,
                    AreaSqFt = x.AreaSqFt,
                    Price = x.Price,
                    Status = x.Status,
                    PropertyType = x.PropertyType,
                    //ThumbnailUrl = x.,
                    //IsSaved = x.IsSaved
                })
                .ToListAsync();

            return new PlotListResponseDto
            {
                Items = items,
                Pagination = new PaginationDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
                }
            };
        }

        public async Task<PlotDetailsDto?> GetPlotByIdAsync(
        Guid plotId,
        string userId)
        {
            var plot = await dbContext.Plots
                .AsNoTracking()
                .Where(x =>
                    x.Id == plotId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .Select(x => new PlotDetailsDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Location = x.Location,
                    AreaSqFt = x.AreaSqFt,
                    Price = x.Price,
                    Status = x.Status,
                    PropertyType = x.PropertyType,
                    RoadWidth = x.RoadWidth,
                    Electricity = x.Electricity,
                    Water = x.Water,
                    Registration = x.Registration,
                    Description = x.Description,

                    Seller = new SellerDto
                    {
                        Name = x.Seller != null
                            ? x.Seller.Name
                            : string.Empty,

                        Phone = x.Seller != null
                            ? x.Seller.Phone
                            : string.Empty
                    },

                    Images = x.Images
                        .Where(image => !image.IsDeleted)
                        .OrderBy(image => image.DisplayOrder)
                        .Select(image => new PlotImageDto
                        {
                            Id = image.Id,
                            ImageUrl = image.ImageUrl,
                            DisplayOrder = image.DisplayOrder
                        })
                        .ToList(),

                    Amenities = x.PlotAmenities
                        .Where(item =>
                            item.Amenity != null &&
                            item.Amenity.IsActive)
                        .OrderBy(item => item.Amenity!.Name)
                        .Select(item => item.Amenity!.Name)
                        .ToList(),

                    IsSaved = x.SavedPlots.Any(saved =>
                        saved.UserId == userId)
                })
                .FirstOrDefaultAsync();

            return plot;
        }

        public async Task<List<SavedPlotDto>> GetSavedPlotsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }

            var savedPlots = await dbContext.SavedPlots
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.Plot != null &&
                    x.Plot.IsActive &&
                    !x.Plot.IsDeleted)
                .OrderByDescending(x => x.SavedAt)
                .Select(x => new SavedPlotDto
                {
                    Id = x.PlotId,
                    Title = x.Plot!.Title,
                    Location = x.Plot.Location,
                    AreaSqFt = x.Plot.AreaSqFt,
                    Price = x.Plot.Price,
                    Status = x.Plot.Status,
                    PropertyType = x.Plot.PropertyType,

                    ThumbnailUrl = x.Plot.Images
                        .Where(image => !image.IsDeleted)
                        .OrderBy(image => image.DisplayOrder)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault() ?? string.Empty,

                    SavedAt = x.SavedAt,
                    IsSaved = true
                })
                .ToListAsync();

            return savedPlots;
        }

        public async Task<SavePlotResponseDto> SavePlotAsync(
        Guid plotId,
        string userId)
        {


            var plotExists = await dbContext.Plots
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == plotId &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (!plotExists)
            {
                throw new Exception(
                    "Plot not found.");
            }

            var existingSavedPlot = await dbContext.SavedPlots
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.PlotId == plotId &&
                    x.UserId == userId);

            if (existingSavedPlot != null)
            {
                throw new Exception(
                    "Plot is already saved.");
            }

            var savedAt = DateTime.UtcNow;

            var savedPlot = new SavedPlot
            {
                PlotId = plotId,
                UserId = userId,
                SavedAt = savedAt
            };

            await dbContext.SavedPlots.AddAsync(savedPlot);
            await dbContext.SaveChangesAsync();

            return new SavePlotResponseDto
            {
                PlotId = savedPlot.PlotId,
                IsSaved = true,
                SavedAt = savedPlot.SavedAt
            };
        }

        public async Task<bool> RemoveSavedPlotAsync(
        Guid plotId,
        string userId)
        {
            var savedPlot = await dbContext.SavedPlots
                .FirstOrDefaultAsync(x =>
                    x.PlotId == plotId &&
                    x.UserId == userId);

            if (savedPlot == null)
            {
                throw new Exception(
                    "Plot is not present in saved plots.");
            }

            dbContext.SavedPlots.Remove(savedPlot);
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<PlotVisitResponseDto> BookSiteVisitAsync(
        Guid plotId,
        string userId,
        BookSiteVisitRequest request)
        {
            var plot = await dbContext.Plots
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x =>
                    x.Id == plotId &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (plot == null)
                throw new Exception("Plot not found.");

            var visit = new PlotVisit
            {
                Id = Guid.NewGuid(),
                PlotId = plotId,
                UserId = userId,
                Name = request.Name,
                MobileNumber = request.MobileNumber,
                VisitDate = request.VisitDate,
                VisitTime = request.VisitTime,
                Remarks = request.Remarks,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.PlotVisits.Add(visit);

            await dbContext.SaveChangesAsync();

            return new PlotVisitResponseDto
            {
                Id = visit.Id,
                PlotId = visit.PlotId,
                PropertyName = plot.Title,
                Name = visit.Name,
                MobileNumber = visit.MobileNumber,
                VisitDate = visit.VisitDate,
                VisitTime = visit.VisitTime,
                Remarks = visit.Remarks,
                Status = visit.Status,
                ImageUrl = plot.Images
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => x.ImageUrl)
                    .FirstOrDefault() ?? "",
                CreatedAt = visit.CreatedAt
            };
        }

        public async Task<PlotVisitListResponseDto> GetMyVisitsAsync(
        string userId,
        string? status,
        int page = 1,
        int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "User ID is required.",
                    nameof(userId));
            }

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = dbContext.PlotVisits
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.Plot != null &&
                    x.Plot.IsActive &&
                    !x.Plot.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusText = status.Trim();

                query = query.Where(x => x.Status == statusText);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.VisitDate)
                .ThenByDescending(x => x.VisitTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PlotVisitListItemDto
                {
                    Id = x.Id,
                    PlotId = x.PlotId,
                    PropertyName = x.Plot!.Title,
                    VisitDate = x.VisitDate,
                    VisitTime = x.VisitTime,
                    Status = x.Status,

                    ImageUrl = x.Plot.Images
                        .Where(image => !image.IsDeleted)
                        .OrderBy(image => image.DisplayOrder)
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            return new PlotVisitListResponseDto
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
