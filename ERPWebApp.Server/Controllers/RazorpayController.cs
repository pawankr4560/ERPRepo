using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Payment;
using WebApp.Service.Razorpay;

namespace ERPWebApp.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RazorpayController : ControllerBase
{
    private readonly IRazorpayService _razorpayService;

    public RazorpayController(IRazorpayService razorpayService)
    {
        _razorpayService = razorpayService;
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        return Ok(_razorpayService.GetConfig());
    }

    [HttpGet("my-loans")]
    public async Task<IActionResult> GetMyLoans()
    {
        var (userId, isAdmin) = GetCurrentUserContext();
        return Ok(await _razorpayService.GetMyLoansAsync(userId, isAdmin));
    }

    [HttpGet("unpaid-installments/{loanId:int}")]
    public async Task<IActionResult> GetUnpaidInstallments(int loanId)
    {
        try
        {
            var (userId, isAdmin) = GetCurrentUserContext();
            return Ok(await _razorpayService.GetUnpaidInstallmentsAsync(loanId, userId, isAdmin));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("emi/order")]
    public async Task<IActionResult> CreateEmiOrder([FromBody] RazorpayEmiOrderRequest request)
    {
        try
        {
            var (userId, isAdmin) = GetCurrentUserContext();
            var result = await _razorpayService.CreateEmiOrderAsync(request, userId, isAdmin);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("emi/verify")]
    public async Task<IActionResult> VerifyEmiPayment([FromBody] RazorpayEmiVerifyRequest request)
    {
        try
        {
            var (userId, isAdmin) = GetCurrentUserContext();
            var result = await _razorpayService.VerifyEmiPaymentAsync(request, userId, isAdmin);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private (string UserId, bool IsAdmin) GetCurrentUserContext()
    {
        var userId = User.FindFirst("Id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("User context is missing.");
        }

        var isAdmin = User.IsInRole("Admin");
        return (userId, isAdmin);
    }
}
