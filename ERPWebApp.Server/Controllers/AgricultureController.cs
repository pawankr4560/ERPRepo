using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Globalization;

namespace WebApp.Server.Controllers;

[Route("api/agriculture")]
[ApiController]
public class AgricultureController : ControllerBase
{
    private static int _fieldSequence = 2;
    private static int _stockSequence = 2;
    private static int _sprayLogSequence = 1;

    private static readonly ConcurrentDictionary<string, FieldRecordDto> Fields = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, StockItemDto> StockItems = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<SprayProductDto> SprayProducts =
    [
        new("spray-chlorpyrifos", "Chlorpyrifos 20% EC", 200m, "ml"),
        new("spray-urea", "Urea granules", 25m, "kg")
    ];

    static AgricultureController()
    {
        Fields.TryAdd("field-north-block", new FieldRecordDto("field-north-block", "North block", "Wheat", 4.5m, "Healthy", "2026-06-28"));
        Fields.TryAdd("field-river-side", new FieldRecordDto("field-river-side", "River side plot", "Cotton", 2.8m, "Needs attention", "2026-06-20"));

        StockItems.TryAdd("stock-chlorpyrifos", new StockItemDto("stock-chlorpyrifos", "Chlorpyrifos 20% EC", 18m, "litres", "Sufficient"));
        StockItems.TryAdd("stock-dap", new StockItemDto("stock-dap", "DAP fertilizer", 0m, "kg", "Out of stock"));
    }

    [HttpGet("dashboard")]
    public IActionResult GetDashboard([FromQuery] string fieldStatus = "All")
    {
        var fields = FilterFields(fieldStatus, null)
            .Select(ToFieldResponse)
            .ToList();

        var stockItems = StockItems.Values
            .Select(ToStockResponse)
            .ToList();

        return Ok(new
        {
            success = true,
            data = new
            {
                fields,
                stockItems
            }
        });
    }

    [HttpGet("fields")]
    public IActionResult GetFields([FromQuery] string? status, [FromQuery] string? crop, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var items = Paginate(FilterFields(status, crop), page, limit)
            .Select(ToFieldResponse)
            .ToList();

        return Ok(new { success = true, data = new { items } });
    }

    [HttpPost("fields")]
    public IActionResult AddField([FromBody] FieldRecordRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var id = ToSlug("field", request.Name, Interlocked.Increment(ref _fieldSequence));
        var field = new FieldRecordDto(id, request.Name, request.Crop, request.AreaAcres, request.Status, request.LastSprayedDate);
        Fields[id] = field;

        return Ok(new
        {
            success = true,
            message = "Field record added successfully",
            data = ToFieldResponse(field)
        });
    }

    [HttpGet("stock")]
    public IActionResult GetStock([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var query = StockItems.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(item => item.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        var items = Paginate(query, page, limit)
            .Select(ToStockResponse)
            .ToList();

        return Ok(new { success = true, data = new { items } });
    }

    [HttpPost("stock")]
    public IActionResult AddStock([FromBody] StockItemRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        var id = ToSlug("stock", request.Name, Interlocked.Increment(ref _stockSequence));
        var stock = new StockItemDto(id, request.Name, request.Quantity, request.Unit, request.Status);
        StockItems[id] = stock;

        return Ok(new
        {
            success = true,
            message = "Stock entry added successfully",
            data = ToStockResponse(stock)
        });
    }

    [HttpPut("stock/{stockId}")]
    public IActionResult UpdateStock(string stockId, [FromBody] StockItemRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        if (!StockItems.ContainsKey(stockId))
            return NotFound(new { success = false, message = "Stock item not found" });

        var stock = new StockItemDto(stockId, request.Name, request.Quantity, request.Unit, request.Status);
        StockItems[stockId] = stock;

        return Ok(new
        {
            success = true,
            message = "Stock entry updated successfully",
            data = ToStockResponse(stock)
        });
    }

    [HttpGet("spray-products")]
    public IActionResult GetSprayProducts()
    {
        var data = SprayProducts
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.DosagePerAcre,
                product.DosageUnit,
                displayLabel = $"{product.Name} - {product.DosagePerAcre:0.#} {product.DosageUnit}/acre"
            })
            .ToList();

        return Ok(new { success = true, data });
    }

    [HttpPost("spray-logs")]
    public IActionResult LogSpray([FromBody] SprayLogRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, message = "Request body is required" });

        if (!Fields.TryGetValue(request.FieldId, out var field))
            return NotFound(new { success = false, message = "Field record not found" });

        var product = SprayProducts.FirstOrDefault(item => item.Id.Equals(request.SprayProductId, StringComparison.OrdinalIgnoreCase));
        if (product == null)
            return BadRequest(new { success = false, message = "Spray product not found" });

        var updatedField = field with
        {
            Status = "Healthy",
            LastSprayedDate = request.ApplicationDate
        };
        Fields[updatedField.Id] = updatedField;

        var data = new
        {
            id = $"spray-log-{Interlocked.Increment(ref _sprayLogSequence):000}",
            fieldId = updatedField.Id,
            fieldName = updatedField.Name,
            sprayProductName = product.Name,
            applicationDate = request.ApplicationDate,
            estimatedDosage = request.EstimatedDosage,
            dosageUnit = request.DosageUnit,
            updatedFieldStatus = updatedField.Status,
            lastSprayedDate = updatedField.LastSprayedDate
        };

        return Ok(new
        {
            success = true,
            message = "Spray application logged successfully",
            data
        });
    }

    private static IEnumerable<FieldRecordDto> FilterFields(string? status, string? crop)
    {
        var query = Fields.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            query = query.Where(field => field.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(crop))
            query = query.Where(field => field.Crop.Equals(crop, StringComparison.OrdinalIgnoreCase));

        return query;
    }

    private static IEnumerable<T> Paginate<T>(IEnumerable<T> source, int page, int limit)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);
        return source.Skip((page - 1) * limit).Take(limit);
    }

    private static object ToFieldResponse(FieldRecordDto field) => new
    {
        field.Id,
        field.Name,
        field.Crop,
        field.AreaAcres,
        field.Status,
        field.LastSprayedDate
    };

    private static object ToStockResponse(StockItemDto item) => new
    {
        item.Id,
        item.Name,
        item.Quantity,
        item.Unit,
        quantityLabel = $"{item.Quantity:0.#} {item.Unit} in stock",
        item.Status
    };

    private static string ToSlug(string prefix, string name, int sequence)
    {
        var slug = new string(name
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(slug) ? $"{prefix}-{sequence:000}" : $"{prefix}-{slug}";
    }

    public record FieldRecordDto(string Id, string Name, string Crop, decimal AreaAcres, string Status, string LastSprayedDate);
    public record StockItemDto(string Id, string Name, decimal Quantity, string Unit, string Status);
    public record SprayProductDto(string Id, string Name, decimal DosagePerAcre, string DosageUnit);

    public class FieldRecordRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Crop { get; set; } = string.Empty;
        public decimal AreaAcres { get; set; }
        public string Status { get; set; } = string.Empty;
        public string LastSprayedDate { get; set; } = string.Empty;
    }

    public class StockItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class SprayLogRequest
    {
        public string FieldId { get; set; } = string.Empty;
        public string SprayProductId { get; set; } = string.Empty;
        public string ApplicationDate { get; set; } = string.Empty;
        public decimal DosagePerAcre { get; set; }
        public string DosageUnit { get; set; } = string.Empty;
        public decimal EstimatedDosage { get; set; }
    }
}
