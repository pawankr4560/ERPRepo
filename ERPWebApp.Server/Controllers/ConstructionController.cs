using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace ERPWebApp.Server.Controllers;

[Route("api/construction")]
[ApiController]
[Authorize]
public class ConstructionController : ControllerBase
{
    private static int _quoteSequence = 1;

    private static readonly List<ConstructionCategoryDto> Categories =
    [
        new("cat_cement", "Cement", "OPC, PPC, bags"),
        new("cat_steel", "Steel", "TMT bars, rods, sheets"),
    ];

    private static readonly List<ConstructionProductDto> Products =
    [
        new("prod_001", "UltraTech PPC Cement", "cat_cement", "Cement", "Bag", "Available", 380m, null),
        new("prod_002", "ACC OPC Cement", "cat_cement", "Cement", "Bag", "Available", 395m, null),
        new("prod_003", "TMT Steel Bar 12mm", "cat_steel", "Steel", "Ton", "Available", 58000m, "Fe 500"),
    ];

    private static readonly ConcurrentBag<ConstructionQuoteDto> Quotes =
    [
        new(
            "quote_001",
            "UltraTech PPC Cement",
            "Cement",
            10,
            "Bag",
            "Site A, Indore",
            "2026-07-09",
            "Pending",
            3800m,
            DateTime.Parse("2026-07-08T11:00:00Z").ToUniversalTime())
    ];

    private static readonly List<ConstructionOrderDto> Orders =
    [
        new("CM-2026-0001", "TMT Steel Bar 12mm", 2, "Ton", 116000m, "Confirmed", "2026-07-08", "Site A, Indore"),
        new("CM-2026-0002", "UltraTech PPC Cement", 25, "Bag", 9500m, "Processing", "2026-07-09", "Site B, Indore"),
    ];

    private static readonly List<ConstructionDeliveryDto> Deliveries =
    [
        new("delivery_001", "CM-2026-0001", "Cement", "UP65 AB 1234", "Ramesh Kumar", "9876543210", "Out for Delivery", "2 hours", 0.72m),
        new("delivery_002", "CM-2026-0002", "Steel", "MP09 CD 4567", "Amit Verma", "9876543211", "Scheduled", "Tomorrow", 0.10m),
    ];

    [HttpGet("dashboard")]
    public IActionResult GetDashboard()
    {
        var data = new
        {
            totalProducts = 48,
            pendingQuotes = Quotes.Count(quote => quote.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)),
            activeOrders = Orders.Count(order => !order.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase)),
            deliveriesToday = 2,
            recentActivities = new[]
            {
                new ConstructionActivityDto(
                    "act_001",
                    "QUOTE",
                    "Quote requested for TMT Steel",
                    "Supplier confirmation pending",
                    DateTime.Parse("2026-07-08T10:30:00Z").ToUniversalTime())
            }
        };

        return Ok(new { success = true, data });
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        return Ok(new { success = true, data = Categories });
    }

    [HttpGet("products")]
    public IActionResult GetProducts(
        [FromQuery] string? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = Products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            query = query.Where(product => product.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                product.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                product.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var total = query.Count();
        var items = query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                items,
                pagination = new
                {
                    page,
                    limit,
                    total = total == 0 ? 48 : total
                }
            }
        });
    }

    [HttpPost("quotes")]
    public IActionResult CreateQuote([FromBody] ConstructionQuoteRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { success = false, message = "Request body is required." });
        }

        var product = Products.FirstOrDefault(item => item.Id.Equals(request.ProductId, StringComparison.OrdinalIgnoreCase));
        if (product == null)
        {
            return BadRequest(new { success = false, message = "Product not found." });
        }

        var quoteId = $"quote_{Interlocked.Increment(ref _quoteSequence):000}";
        var createdAt = DateTime.UtcNow;
        var quote = new ConstructionQuoteDto(
            quoteId,
            product.Name,
            product.CategoryName,
            request.Quantity,
            request.Unit,
            request.DeliveryLocation,
            request.RequiredDate,
            "Pending",
            product.Rate * request.Quantity,
            createdAt);

        Quotes.Add(quote);

        var data = new
        {
            quoteId = quote.Id,
            status = quote.Status,
            productName = quote.ProductName,
            quantity = quote.Quantity,
            unit = quote.Unit,
            deliveryLocation = quote.DeliveryLocation,
            requiredDate = quote.RequiredDate,
            createdAt = quote.CreatedAt
        };

        return Ok(new
        {
            success = true,
            message = "Quote request submitted successfully",
            data
        });
    }

    [HttpGet("quotes")]
    public IActionResult GetQuotes(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = Quotes.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(quote => quote.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        var items = query
            .OrderByDescending(quote => quote.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        return Ok(new { success = true, data = new { items } });
    }

    [HttpGet("orders")]
    public IActionResult GetOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = Orders.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(order => order.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        var items = query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        return Ok(new { success = true, data = new { items } });
    }

    [HttpGet("deliveries")]
    public IActionResult GetDeliveries(
        [FromQuery] string? status,
        [FromQuery] string? date)
    {
        var query = Deliveries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(delivery => delivery.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(new { success = true, data = query.ToList() });
    }

    public record ConstructionActivityDto(string Id, string Type, string Title, string Subtitle, DateTime CreatedAt);
    public record ConstructionCategoryDto(string Id, string Name, string Subtitle);
    public record ConstructionProductDto(string Id, string Name, string CategoryId, string CategoryName, string Unit, string StockStatus, decimal Rate, string? Grade);
    public record ConstructionQuoteDto(string Id, string ProductName, string CategoryName, int Quantity, string Unit, string DeliveryLocation, string RequiredDate, string Status, decimal EstimatedAmount, DateTime CreatedAt);
    public record ConstructionOrderDto(string Id, string Material, int Quantity, string Unit, decimal Amount, string Status, string DeliveryDate, string DeliveryLocation);
    public record ConstructionDeliveryDto(string Id, string OrderId, string Material, string VehicleNumber, string DriverName, string DriverPhone, string Status, string Eta, decimal Progress);

    public class ConstructionQuoteRequest
    {
        public string CategoryId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string DeliveryLocation { get; set; } = string.Empty;
        public string RequiredDate { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
