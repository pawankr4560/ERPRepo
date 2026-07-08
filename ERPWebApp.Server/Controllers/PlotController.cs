using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Globalization;

namespace WebApp.Server.Controllers;

[Route("api/plots")]
[ApiController]
public class PlotController : ControllerBase
{
    private static int _visitSequence = 1;

    private static readonly ConcurrentDictionary<string, DateTime> SavedPlots = new(StringComparer.OrdinalIgnoreCase);

    private static readonly List<PlotDetailDto> Plots =
    [
        new(
            "green-valley-a12",
            "Green Valley Premium Plot",
            "Indore Bypass, Madhya Pradesh",
            1500,
            "sq.ft",
            4250000m,
            "Available",
            "Residential Plot",
            "https://example.com/plot-1.jpg",
            ["Gated", "Road", "Water", "Electricity"],
            true,
            "30 ft",
            "Available",
            "Borewell + municipal line",
            "RERA verified",
            "A well-planned residential plot inside a gated community.",
            new PlotSellerDto("seller_001", "Amit Verma", "+91 98765 43210"),
            ["https://example.com/plot-1.jpg", "https://example.com/plot-2.jpg"],
            new PlotLocationMapDto(22.7196m, 75.8577m, "Indore Bypass, Madhya Pradesh")),
        new(
            "super-corridor-b08",
            "Super Corridor Corner Plot",
            "Super Corridor, Indore",
            1200,
            "sq.ft",
            3600000m,
            "Available",
            "Residential Plot",
            "https://example.com/plot-2.jpg",
            ["Road", "Water"],
            true,
            "40 ft",
            "Available",
            "Municipal line",
            "Verified",
            "Corner plot near main access road.",
            new PlotSellerDto("seller_002", "Rahul Jain", "+91 98765 43211"),
            ["https://example.com/plot-2.jpg"],
            new PlotLocationMapDto(22.7533m, 75.8937m, "Super Corridor, Indore")),
        new(
            "rau-residency-c21",
            "Rau Residency Plot",
            "Rau, Indore",
            1800,
            "sq.ft",
            5900000m,
            "Available",
            "Residential Plot",
            "https://example.com/plot-3.jpg",
            ["Gated", "Electricity"],
            true,
            "30 ft",
            "Available",
            "Borewell",
            "RERA verified",
            "Large residential plot with good road connectivity.",
            new PlotSellerDto("seller_003", "Pooja Sharma", "+91 98765 43212"),
            ["https://example.com/plot-3.jpg"],
            new PlotLocationMapDto(22.6333m, 75.8064m, "Rau, Indore")),
        new(
            "dewas-naka-d04",
            "Dewas Naka Commercial Plot",
            "Dewas Naka, Indore",
            2000,
            "sq.ft",
            6000000m,
            "Available",
            "Commercial Plot",
            "https://example.com/plot-4.jpg",
            ["Road", "Water", "Electricity"],
            false,
            "45 ft",
            "Available",
            "Municipal line",
            "Verified",
            "Commercial-facing plot suitable for warehouse or shop.",
            new PlotSellerDto("seller_004", "Neha Verma", "+91 98765 43213"),
            ["https://example.com/plot-4.jpg"],
            new PlotLocationMapDto(22.7636m, 75.9124m, "Dewas Naka, Indore"))
    ];

    private static readonly ConcurrentDictionary<string, PlotVisitDto> Visits = new(StringComparer.OrdinalIgnoreCase);

    static PlotController()
    {
        SavedPlots.TryAdd("green-valley-a12", DateTime.Parse("2026-07-08T10:30:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));
        Visits.TryAdd("visit_001", new PlotVisitDto(
            "visit_001",
            "green-valley-a12",
            "Green Valley Premium Plot",
            "2026-07-09",
            "11:00",
            "Pending",
            "https://example.com/plot-1.jpg",
            DateTime.Parse("2026-07-08T11:00:00Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)));
    }

    [HttpGet]
    public IActionResult GetPlots(
        [FromQuery] string? search,
        [FromQuery] string? location,
        [FromQuery] decimal? minBudget,
        [FromQuery] decimal? maxBudget,
        [FromQuery] int? minArea,
        [FromQuery] int? maxArea,
        [FromQuery] string? propertyType,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = Plots.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(plot =>
                plot.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                plot.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(plot => plot.Location.Contains(location, StringComparison.OrdinalIgnoreCase));

        if (minBudget.HasValue)
            query = query.Where(plot => plot.Price >= minBudget.Value);

        if (maxBudget.HasValue)
            query = query.Where(plot => plot.Price <= maxBudget.Value);

        if (minArea.HasValue)
            query = query.Where(plot => plot.Area >= minArea.Value);

        if (maxArea.HasValue)
            query = query.Where(plot => plot.Area <= maxArea.Value);

        if (!string.IsNullOrWhiteSpace(propertyType))
            query = query.Where(plot => plot.PropertyType.Equals(propertyType, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(plot => plot.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        var filtered = query.ToList();
        var items = filtered
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(ToListItem)
            .ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                total = filtered.Count,
                items,
                pagination = new
                {
                    page,
                    limit,
                    totalPages = filtered.Count == 0 ? 0 : (int)Math.Ceiling(filtered.Count / (double)limit)
                }
            }
        });
    }

    [HttpGet("saved")]
    public IActionResult GetSavedPlots()
    {
        var data = SavedPlots
            .Select(saved =>
            {
                var plot = Plots.FirstOrDefault(item => item.Id.Equals(saved.Key, StringComparison.OrdinalIgnoreCase));
                return plot == null
                    ? null
                    : new
                    {
                        id = plot.Id,
                        title = plot.Title,
                        location = plot.Location,
                        displayArea = FormatArea(plot.Area, plot.AreaUnit),
                        displayPrice = FormatPrice(plot.Price),
                        status = plot.Status,
                        thumbnailUrl = plot.ThumbnailUrl,
                        savedAt = saved.Value
                    };
            })
            .Where(item => item != null)
            .ToList();

        return Ok(new { success = true, data });
    }

    [HttpGet("visits")]
    public IActionResult GetVisits([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = Visits.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(visit => visit.status.Equals(status, StringComparison.OrdinalIgnoreCase));

        var items = query
            .OrderByDescending(visit => visit.createdAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(visit => new
            {
                visit.visitId,
                visit.plotId,
                visit.propertyName,
                visit.visitDate,
                displayDate = FormatDisplayDate(visit.visitDate),
                visit.visitTime,
                displayTime = FormatDisplayTime(visit.visitTime),
                visit.status,
                imageUrl = visit.imageUrl
            })
            .ToList();

        return Ok(new { success = true, data = new { items } });
    }

    [HttpGet("{plotId}")]
    public IActionResult GetPlot(string plotId)
    {
        var plot = Plots.FirstOrDefault(item => item.Id.Equals(plotId, StringComparison.OrdinalIgnoreCase));
        if (plot == null)
            return NotFound(new { success = false, message = "Plot not found" });

        return Ok(new { success = true, data = ToDetail(plot) });
    }

    [HttpPost("{plotId}/save")]
    public IActionResult SavePlot(string plotId)
    {
        if (!PlotExists(plotId))
            return NotFound(new { success = false, message = "Plot not found" });

        SavedPlots.AddOrUpdate(plotId, DateTime.UtcNow, (_, _) => DateTime.UtcNow);

        return Ok(new
        {
            success = true,
            message = "Plot saved successfully",
            data = new { plotId, isSaved = true }
        });
    }

    [HttpDelete("{plotId}/save")]
    public IActionResult RemoveSavedPlot(string plotId)
    {
        if (!PlotExists(plotId))
            return NotFound(new { success = false, message = "Plot not found" });

        SavedPlots.TryRemove(plotId, out _);

        return Ok(new
        {
            success = true,
            message = "Plot removed from saved list",
            data = new { plotId, isSaved = false }
        });
    }

    [HttpPost("{plotId}/visits")]
    public IActionResult BookVisit(string plotId, [FromBody] PlotVisitRequest request)
    {
        var plot = Plots.FirstOrDefault(item => item.Id.Equals(plotId, StringComparison.OrdinalIgnoreCase));
        if (plot == null)
            return NotFound(new { success = false, message = "Plot not found" });

        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var visitId = $"visit_{Interlocked.Increment(ref _visitSequence):000}";
        var visit = new PlotVisitDto(
            visitId,
            plot.Id,
            plot.Title,
            request.VisitDate,
            request.VisitTime,
            "Pending",
            plot.ThumbnailUrl,
            DateTime.UtcNow);

        Visits[visitId] = visit;

        return Ok(new
        {
            success = true,
            message = "Visit booked successfully",
            data = new
            {
                visitId = visit.visitId,
                plotId = visit.plotId,
                propertyName = visit.propertyName,
                visitDate = visit.visitDate,
                visitTime = visit.visitTime,
                status = visit.status,
                createdAt = visit.createdAt
            }
        });
    }

    [HttpPatch("visits/{visitId}/cancel")]
    public IActionResult CancelVisit(string visitId, [FromBody] CancelVisitRequest request)
    {
        if (!Visits.TryGetValue(visitId, out var visit))
            return NotFound(new { success = false, message = "Visit not found" });

        var cancelled = visit with { status = "Cancelled" };
        Visits[visitId] = cancelled;

        return Ok(new
        {
            success = true,
            message = "Visit cancelled successfully",
            data = new
            {
                visitId,
                status = cancelled.status
            }
        });
    }

    private static bool PlotExists(string plotId) =>
        Plots.Any(plot => plot.Id.Equals(plotId, StringComparison.OrdinalIgnoreCase));

    private static object ToListItem(PlotDetailDto plot) => new
    {
        id = plot.Id,
        title = plot.Title,
        location = plot.Location,
        area = plot.Area,
        areaUnit = plot.AreaUnit,
        displayArea = FormatArea(plot.Area, plot.AreaUnit),
        price = plot.Price,
        displayPrice = FormatPrice(plot.Price),
        status = plot.Status,
        propertyType = plot.PropertyType,
        thumbnailUrl = plot.ThumbnailUrl,
        amenities = plot.Amenities.Take(3).ToArray(),
        isSaved = SavedPlots.ContainsKey(plot.Id),
        isVerified = plot.IsVerified
    };

    private static object ToDetail(PlotDetailDto plot) => new
    {
        id = plot.Id,
        title = plot.Title,
        location = plot.Location,
        area = plot.Area,
        areaUnit = plot.AreaUnit,
        displayArea = FormatArea(plot.Area, plot.AreaUnit),
        price = plot.Price,
        displayPrice = FormatPrice(plot.Price),
        status = plot.Status,
        propertyType = plot.PropertyType,
        roadWidth = plot.RoadWidth,
        electricity = plot.Electricity,
        water = plot.Water,
        registration = plot.Registration,
        description = plot.Description,
        seller = plot.Seller,
        images = plot.Images,
        amenities = plot.Amenities,
        locationMap = plot.LocationMap,
        isSaved = SavedPlots.ContainsKey(plot.Id),
        isVerified = plot.IsVerified
    };

    private static string FormatArea(int area, string unit) => string.Format(CultureInfo.InvariantCulture, "{0:N0} {1}", area, unit);

    private static string FormatPrice(decimal price)
    {
        if (price >= 10000000)
            return $"Rs. {price / 10000000m:0.#}Cr";

        return $"Rs. {price / 100000m:0.#}L";
    }

    private static string FormatDisplayDate(string value) =>
        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
            : value;

    private static string FormatDisplayTime(string value) =>
        DateTime.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
            ? time.ToString("hh:mm tt", CultureInfo.InvariantCulture)
            : value;

    public record PlotSellerDto(string Id, string Name, string Phone);
    public record PlotLocationMapDto(decimal Latitude, decimal Longitude, string Address);
    public record PlotDetailDto(
        string Id,
        string Title,
        string Location,
        int Area,
        string AreaUnit,
        decimal Price,
        string Status,
        string PropertyType,
        string ThumbnailUrl,
        string[] Amenities,
        bool IsVerified,
        string RoadWidth,
        string Electricity,
        string Water,
        string Registration,
        string Description,
        PlotSellerDto Seller,
        string[] Images,
        PlotLocationMapDto LocationMap);

    public record PlotVisitDto(
        string visitId,
        string plotId,
        string propertyName,
        string visitDate,
        string visitTime,
        string status,
        string imageUrl,
        DateTime createdAt);

    public class PlotVisitRequest
    {
        public string Name { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string VisitDate { get; set; } = string.Empty;
        public string VisitTime { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class CancelVisitRequest
    {
        public string? Reason { get; set; }
    }
}

