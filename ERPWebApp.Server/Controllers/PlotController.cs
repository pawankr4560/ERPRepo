using ERPWebAppModels.Plot;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Claims;
using WebApp.Model.Common;
using WebApp.Service.Plot;

namespace WebApp.Server.Controllers;

[Route("api/plots")]
[ApiController]
public class PlotController : ControllerBase
{
    private readonly IPlotService plotService;

    public PlotController(IPlotService plotService)
    {
        this.plotService = plotService;
    }
    [HttpGet]
    public async Task<IActionResult> GetPlots(
    [FromQuery] string? search,
    [FromQuery] string? status,
    [FromQuery] string? location,
    [FromQuery] string? propertyType,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] decimal? minArea,
    [FromQuery] decimal? maxArea,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        if (page <= 0)
        {
            return BadRequest(new ApiResponse(
                false,
                "Page must be greater than zero.",
                null));
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            return BadRequest(new ApiResponse(
                false,
                "Page size must be between 1 and 100.",
                null));
        }

        if (minPrice.HasValue && minPrice.Value < 0)
        {
            return BadRequest(new ApiResponse(
                false,
                "Minimum price cannot be negative.",
                null));
        }

        if (maxPrice.HasValue && maxPrice.Value < 0)
        {
            return BadRequest(new ApiResponse(
                false,
                "Maximum price cannot be negative.",
                null));
        }

        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            return BadRequest(new ApiResponse(
                false,
                "Minimum price cannot be greater than maximum price.",
                null));
        }

        if (minArea.HasValue && minArea.Value <= 0)
        {
            return BadRequest(new ApiResponse(
                false,
                "Minimum area must be greater than zero.",
                null));
        }

        if (maxArea.HasValue && maxArea.Value <= 0)
        {
            return BadRequest(new ApiResponse(
                false,
                "Maximum area must be greater than zero.",
                null));
        }

        if (minArea.HasValue &&
            maxArea.HasValue &&
            minArea.Value > maxArea.Value)
        {
            return BadRequest(new ApiResponse(
                false,
                "Minimum area cannot be greater than maximum area.",
                null));
        }

        var result = await plotService.GetPlotsAsync(
            search,
            status,
            location,
            propertyType,
            minPrice,
            maxPrice,
            minArea,
            maxArea,
            page,
            pageSize);

        return Ok(new ApiResponse(
            true,
            "Plots retrieved successfully.",
            result));
    }

    [HttpGet("{plotId:guid}")]
    public async Task<IActionResult> GetPlotById(Guid plotId)
    {
        if (plotId == Guid.Empty)
        {
            return BadRequest(new ApiResponse(
                false,
                "A valid plot ID is required.",
                null));
        }

        var result = await plotService.GetPlotByIdAsync(
            plotId,
            GetUserId());

        if (result == null)
        {
            return NotFound(new ApiResponse(
                false,
                "Plot not found.",
                null));
        }

        return Ok(new ApiResponse(
            true,
            "Plot details retrieved successfully.",
            result));
    }

    [HttpGet("saved")]
    public async Task<IActionResult> GetSavedPlots()
    {
        var result = await plotService.GetSavedPlotsAsync(GetUserId());

        return Ok(new ApiResponse(
            true,
            result.Count > 0
                ? "Saved plots retrieved successfully."
                : "No saved plots found.",
            result));
    }

    [HttpPost("{plotId:guid}/save")]
    public async Task<IActionResult> SavePlot(Guid plotId)
    {
        if (plotId == Guid.Empty)
        {
            return BadRequest(new ApiResponse(
                false,
                "A valid plot ID is required.",
                null));
        }

        var result = await plotService.SavePlotAsync(
            plotId,
            GetUserId());

        return StatusCode(
            StatusCodes.Status201Created,
            new ApiResponse(
                true,
                "Plot saved successfully.",
                result));
    }

    [HttpDelete("{plotId:guid}/save")]
    public async Task<IActionResult> RemoveSavedPlot(Guid plotId)
    {
        if (plotId == Guid.Empty)
        {
            return BadRequest(new ApiResponse(
                false,
                "A valid plot ID is required.",
                null));
        }

        var result = await plotService.RemoveSavedPlotAsync(
            plotId,
            GetUserId());

        return Ok(new ApiResponse(
            true,
            "Plot removed from saved plots.",
            null));
    }

    [HttpPost("{plotId:guid}/visits")]
    public async Task<IActionResult> BookSiteVisit(Guid plotId,BookSiteVisitRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse(
                false,
                "Invalid request.",
                ModelState));
        }

        var result = await plotService.BookSiteVisitAsync(
            plotId,
            GetUserId(),
            request);

        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse(
                true,
                "Site visit booked successfully.",
                result));
    }

    [HttpGet("visits")]
    public async Task<IActionResult> GetMyVisits(
    [FromQuery] string? status,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        if (page <= 0)
        {
            return BadRequest(new ApiResponse(
                false,
                "Page must be greater than zero.",
                null));
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            return BadRequest(new ApiResponse(
                false,
                "Page size must be between 1 and 100.",
                null));
        }

        var allowedStatuses = new[]
        {
        "Pending",
        "Confirmed",
        "Completed",
        "Cancelled"
    };

        if (!string.IsNullOrWhiteSpace(status) &&
            !allowedStatuses.Contains(
                status.Trim(),
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new ApiResponse(
                false,
                "Invalid visit status.",
                null));
        }
        
        var result = await plotService.GetMyVisitsAsync(
            GetUserId(),
            status,
            page,
            pageSize);

        return Ok(new ApiResponse(
            true,
            "Site visits retrieved successfully.",
            result));
    }
    private string GetUserId()
    {
        return User.FindFirst("Id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;
    }
}

