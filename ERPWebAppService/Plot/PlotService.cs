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
                .Where(x => x.IsActive && !x.IsDeleted)
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
            if (!plot.Status.Equals("Available", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This plot is not available for booking.");

            var duplicateExists = await dbContext.PlotVisits.AsNoTracking().AnyAsync(x =>
                x.PlotId == plotId && x.UserId == userId && x.VisitDate == request.VisitDate &&
                (x.Status == "Pending" || x.Status == "Confirmed"));
            if (duplicateExists)
                throw new InvalidOperationException("You already have an active booking for this plot on the selected date.");

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

        public async Task<List<AdminPlotDto>> GetAdminPlotsAsync(string? search)
        {
            var query = dbContext.Plots.AsNoTracking().Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(x => x.Title.Contains(value) || x.Location.Contains(value));
            }

            return await query.OrderByDescending(x => x.UpdatedAt).Select(x => new AdminPlotDto
            {
                Id = x.Id, Title = x.Title, Location = x.Location, AreaSqFt = x.AreaSqFt,
                Price = x.Price, Status = x.Status, PropertyType = x.PropertyType,
                RoadWidth = x.RoadWidth, Electricity = x.Electricity, Water = x.Water,
                Registration = x.Registration, Description = x.Description,
                ThumbnailUrl = x.ThumbnailUrl, SellerName = x.Seller != null ? x.Seller.Name : "",
                SellerPhone = x.Seller != null ? x.Seller.Phone : "", IsActive = x.IsActive,
                CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt,
                ImageUrls = x.Images.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
                AmenityIds = x.PlotAmenities.Select(a => a.AmenityId).ToList()
            }).ToListAsync();
        }

        public async Task<AdminPlotDto> CreatePlotAsync(AdminPlotRequest request)
        {
            var seller = await FindOrCreateSellerAsync(request.SellerName, request.SellerPhone);
            var plot = new ERPWebAppData.Entity.Plot { Id = Guid.NewGuid(), SellerId = seller.Id };
            Apply(plot, request);
            dbContext.Plots.Add(plot);
            SyncImages(plot, request.ImageUrls);
            SyncAmenities(plot, request.AmenityIds);
            await dbContext.SaveChangesAsync();
            return ToAdminDto(plot, seller);
        }

        public async Task<AdminPlotDto?> UpdatePlotAsync(Guid plotId, AdminPlotRequest request)
        {
            var plot = await dbContext.Plots.Include(x => x.Seller).Include(x => x.Images).Include(x => x.PlotAmenities)
                .FirstOrDefaultAsync(x => x.Id == plotId && !x.IsDeleted);
            if (plot == null) return null;
            var seller = await FindOrCreateSellerAsync(request.SellerName, request.SellerPhone);
            plot.SellerId = seller.Id;
            plot.Seller = seller;
            Apply(plot, request);
            SyncImages(plot, request.ImageUrls);
            SyncAmenities(plot, request.AmenityIds);
            await dbContext.SaveChangesAsync();
            return ToAdminDto(plot, seller);
        }

        public async Task<bool> DeletePlotAsync(Guid plotId)
        {
            var plot = await dbContext.Plots.FirstOrDefaultAsync(x => x.Id == plotId && !x.IsDeleted);
            if (plot == null) return false;
            plot.IsDeleted = true;
            plot.IsActive = false;
            plot.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return true;
        }

        private async Task<Seller> FindOrCreateSellerAsync(string name, string phone)
        {
            var trimmedPhone = phone.Trim();
            var seller = await dbContext.Sellers.FirstOrDefaultAsync(x => x.Phone == trimmedPhone);
            if (seller != null) return seller;
            seller = new Seller { Id = Guid.NewGuid(), Name = name.Trim(), Phone = trimmedPhone, IsActive = true };
            dbContext.Sellers.Add(seller);
            return seller;
        }

        private static void Apply(ERPWebAppData.Entity.Plot plot, AdminPlotRequest request)
        {
            plot.Title = request.Title.Trim(); plot.Location = request.Location.Trim();
            plot.AreaSqFt = request.AreaSqFt; plot.Price = request.Price;
            plot.Status = request.Status.Trim(); plot.PropertyType = request.PropertyType.Trim();
            plot.RoadWidth = request.RoadWidth.Trim(); plot.Electricity = request.Electricity.Trim();
            plot.Water = request.Water.Trim(); plot.Registration = request.Registration.Trim();
            plot.Description = request.Description.Trim();
            plot.ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl)
                ? request.ImageUrls.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim()
                : request.ThumbnailUrl.Trim();
            plot.IsActive = request.IsActive; plot.UpdatedAt = DateTime.UtcNow;
        }

        private static AdminPlotDto ToAdminDto(ERPWebAppData.Entity.Plot x, Seller seller) => new()
        {
            Id = x.Id, Title = x.Title, Location = x.Location, AreaSqFt = x.AreaSqFt,
            Price = x.Price, Status = x.Status, PropertyType = x.PropertyType,
            RoadWidth = x.RoadWidth, Electricity = x.Electricity, Water = x.Water,
            Registration = x.Registration, Description = x.Description, ThumbnailUrl = x.ThumbnailUrl,
            SellerName = seller.Name, SellerPhone = seller.Phone, IsActive = x.IsActive,
            CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt,
            ImageUrls = x.Images.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
            AmenityIds = x.PlotAmenities.Select(a => a.AmenityId).ToList()
        };

        private static void SyncImages(ERPWebAppData.Entity.Plot plot, IEnumerable<string> urls)
        {
            foreach (var image in plot.Images) image.IsDeleted = true;
            var cleanUrls = urls.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().Take(20).ToList();
            for (var index = 0; index < cleanUrls.Count; index++)
            {
                var existing = plot.Images.FirstOrDefault(x => x.ImageUrl == cleanUrls[index]);
                if (existing != null) { existing.IsDeleted = false; existing.DisplayOrder = index; }
                else plot.Images.Add(new PlotImage { Id = Guid.NewGuid(), PlotId = plot.Id, ImageUrl = cleanUrls[index], DisplayOrder = index });
            }
        }

        private static void SyncAmenities(ERPWebAppData.Entity.Plot plot, IEnumerable<int> amenityIds)
        {
            var wanted = amenityIds.Where(x => x > 0).Distinct().ToHashSet();
            var removed = plot.PlotAmenities.Where(x => !wanted.Contains(x.AmenityId)).ToList();
            foreach (var item in removed) plot.PlotAmenities.Remove(item);
            foreach (var id in wanted.Where(id => plot.PlotAmenities.All(x => x.AmenityId != id)))
                plot.PlotAmenities.Add(new PlotAmenity { Id = Guid.NewGuid(), PlotId = plot.Id, AmenityId = id });
        }

        public Task<List<AdminAmenityDto>> GetAmenitiesAsync() => dbContext.Amenities.AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new AdminAmenityDto { Id = x.Id, Name = x.Name }).ToListAsync();

        public async Task<List<AdminPlotVisitDto>> GetAdminVisitsAsync(string? status, string? search = null)
        {
            // Administrators need the complete booking history, even when a listing was hidden or deleted later.
            var query = dbContext.PlotVisits.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status.Trim());
            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(x => x.Name.Contains(value) || x.MobileNumber.Contains(value) ||
                    (x.Plot != null && x.Plot.Title.Contains(value)));
            }
            return await query.OrderByDescending(x => x.VisitDate).ThenByDescending(x => x.VisitTime)
                .Select(x => new AdminPlotVisitDto { Id=x.Id, PlotId=x.PlotId, PlotTitle=x.Plot != null ? x.Plot.Title : "Deleted plot",
                    PlotLocation=x.Plot != null ? x.Plot.Location : "", PlotAreaSqFt=x.Plot != null ? x.Plot.AreaSqFt : 0,
                    PlotPrice=x.Plot != null ? x.Plot.Price : 0, PlotStatus=x.Plot != null ? x.Plot.Status : "Deleted",
                    CustomerEmail=x.User != null ? (x.User.Email ?? "") : "",
                    CustomerName=x.Name, MobileNumber=x.MobileNumber, VisitDate=x.VisitDate, VisitTime=x.VisitTime,
                    Remarks=x.Remarks, Status=x.Status, CreatedAt=x.CreatedAt }).ToListAsync();
        }

        public async Task<AdminPlotVisitDto?> UpdateVisitStatusAsync(Guid visitId, string status)
        {
            var visit = await dbContext.PlotVisits.Include(x => x.Plot).FirstOrDefaultAsync(x => x.Id == visitId);
            if (visit == null) return null;
            visit.Status = status;
            await dbContext.SaveChangesAsync();
            return new AdminPlotVisitDto { Id=visit.Id, PlotId=visit.PlotId, PlotTitle=visit.Plot?.Title ?? "",
                PlotLocation=visit.Plot?.Location ?? "", PlotAreaSqFt=visit.Plot?.AreaSqFt ?? 0,
                PlotPrice=visit.Plot?.Price ?? 0, PlotStatus=visit.Plot?.Status ?? "Deleted",
                CustomerName=visit.Name, MobileNumber=visit.MobileNumber, VisitDate=visit.VisitDate,
                VisitTime=visit.VisitTime, Remarks=visit.Remarks, Status=visit.Status, CreatedAt=visit.CreatedAt };
        }
    }
}
