using ERPWebAppModels.Construction;
using ERPWebAppService.Construction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Security.Claims;
using WebApp.Data.Entity;
using WebApp.Model.Common;

namespace ERPWebApp.Server.Controllers;

[Route("api/construction")]
[ApiController]
[Authorize]
public class ConstructionController : ControllerBase
{
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

    [HttpGet("units")]
    public async Task<IActionResult> GetUnits()
    {
        try
        {
            var data = await constService.Units();
            return Ok(new ApiResponse(true, null, data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpPut("quotes/{quoteId:int}/price")]
    public async Task<IActionResult> GetUnits(int quoteId,[FromBody] UpdateQuotePriceRequest request)
    {
        try
        {
            var data = await constService.UpdateQuotePrice(quoteId,request);
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
    public async Task<IActionResult> GetQuotes()
    {
        try
        {
            var items = await constService.GetQuotes(GetUserId());
            return Ok(new ApiResponse(true, null, items));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(int quoteId)
    {
        try
        {
           var data = await constService.GetOrders(GetUserId());
            return Ok(new ApiResponse(true, null, data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpPost("orders/from-quote/{quoteId:int}")]
    public async Task<IActionResult> CreateOrderFromQuote(int quoteId)
    {
        try
        {
            var data = await constService.CreateOrderFromQuote(
             quoteId,
             GetUserId());
            return Ok(new ApiResponse(true, null, data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    [HttpGet("deliveries")]
    public async Task<IActionResult> GetDeliveries()
    {
        try
        {
            var data = await constService.GetDeliveries(GetUserId());
            return Ok(new ApiResponse(true, null, data));
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message, null));
        }
    }

    private string GetUserId()
    {
        return User.FindFirst("Id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }
}
