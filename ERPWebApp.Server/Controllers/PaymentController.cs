using ERPWebAppService.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERPWebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "Unauthorized"
                });
            }

            var result = await _paymentService.GetUserPaymentsAsync(userId);

            return Ok(result);
        }

        private string GetUserId()
        {
            return User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }
    }
}



