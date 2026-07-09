using ERPWebAppModels.Construction;
using ERPWebAppService.Construction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Security.Claims;
using WebApp.Model.Common;

namespace ERPWebApp.Server.Controllers;

[Route("api/construction")]
[ApiController]
[Authorize]
public class ConstructionController : ControllerBase
{
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

    private readonly IConstructionService constService;

    public ConstructionController(IConstructionService constService)
    {
        this.constService = constService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var data = await constService.DashbordData(GetUserId());
            return Ok(new ApiResponse(true, null, data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var data = await constService.GetCategories();
            return Ok(new ApiResponse(true, null, data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            var data = await constService.GetProducts(categoryId, search, page, limit);
            return Ok(new ApiResponse(true, null, data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpPost("quotes")]
    public async Task<IActionResult> CreateQuote([FromBody] ConstructionQuoteRequest request)
    {
        try
        {
            var data = await constService.CreateQuote(request, GetUserId());
            return Ok(new ApiResponse(true, "Quote request submitted successfully", data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpGet("quotes")]
    public IActionResult GetQuotes(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
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

            return Ok(new ApiResponse(true, null, new { items }));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpGet("orders")]
    public IActionResult GetOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
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

            return Ok(new ApiResponse(true, null, new { items }));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpGet("deliveries")]
    public IActionResult GetDeliveries(
        [FromQuery] string? status,
        [FromQuery] string? date)
    {
        try
        {
            var query = Deliveries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(delivery => delivery.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(new ApiResponse(true, null, query.ToList()));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    public record ConstructionActivityDto(string Id, string Type, string Title, string Subtitle, DateTime CreatedAt);
    public record ConstructionCategoryDto(string Id, string Name, string Subtitle);
    public record ConstructionProductDto(string Id, string Name, string CategoryId, string CategoryName, string Unit, string StockStatus, decimal Rate, string? Grade);
    public record ConstructionQuoteDto(string Id, string ProductName, string CategoryName, int Quantity, string Unit, string DeliveryLocation, string RequiredDate, string Status, decimal EstimatedAmount, DateTime CreatedAt);
    public record ConstructionOrderDto(string Id, string Material, int Quantity, string Unit, decimal Amount, string Status, string DeliveryDate, string DeliveryLocation);
    public record ConstructionDeliveryDto(string Id, string OrderId, string Material, string VehicleNumber, string DriverName, string DriverPhone, string Status, string Eta, decimal Progress);

    private string GetUserId()
    {
        return User.FindFirst("Id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }
}
