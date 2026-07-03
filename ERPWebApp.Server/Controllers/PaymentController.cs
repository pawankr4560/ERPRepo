using ERPWebAppService.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new
                {
                    message = "UserId is required."
                });
            }

            var result = await _paymentService.GetUserPaymentsAsync(userId);

            return Ok(result);
        }
    }
}
