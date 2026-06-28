using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Common;
using WebApp.Model.Order;
using WebApp.Service.Order;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder(List<CreateOrderRequestModel> model)
        {
            try
            {
                var result = await _orderService.CreateOrder(model);
                return Ok(new ApiResponse(true, null, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpGet("OrderList")]
        public async Task<IActionResult> Orders()
        {
            try
            {
                var result = await _orderService.GetOrders();
                return Ok(new ApiResponse(true, null, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpGet("SalesItems")]
        public async Task<IActionResult> SalesItems()
        {
            try
            {
                var result = await _orderService.GetSalesItemsAsync();
                return Ok(new ApiResponse(true, null, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpPost("SalesCheckout")]
        public async Task<IActionResult> SalesCheckout([FromBody] SalesOrderCheckoutRequest model)
        {
            try
            {
                var result = await _orderService.CreateSalesOrderCheckoutAsync(model, GetCurrentUserId());
                return Ok(new ApiResponse(true, null, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpPost("VerifySalesPayment")]
        public async Task<IActionResult> VerifySalesPayment([FromBody] SalesOrderVerifyRequest model)
        {
            try
            {
                var result = await _orderService.VerifySalesOrderPaymentAsync(model, GetCurrentUserId());
                return result.Success
                    ? Ok(new ApiResponse(true, result.Message, result))
                    : BadRequest(new ApiResponse(false, result.Message, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst("Id")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException("User context is missing.");
            }

            return userId;
        }
    }
}
