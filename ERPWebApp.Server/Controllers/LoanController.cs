using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApp.Model.Transaction;
using WebApp.Service.Message;
using WebApp.Service.Transaction;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly IMessageService _messageService;

        public LoanController(ILoanService loanService,IMessageService messageService)
        {
            _loanService = loanService;
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _loanService.LoanList());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var loan = await _loanService.GetLoanById(id);

            return loan == null ? NotFound() : Ok(loan);
        }

        [HttpGet("loan-data")]
        public async Task<IActionResult> GetLoanData()
        {
            var result = await _loanService.GetLoanNumber();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoanRequestModel model)
        {
            var result = await _loanService.AddLoan(model);

            return result ? Ok() : BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] LoanRequestModel model)
        {
            var result = await _loanService.UpdateLoan(model);

            return result ? Ok() : NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? string.Empty;
                var result = await _loanService.ApproveLoan(id, userId);
                return result
                    ? Ok(new { success = true, message = "Loan approved successfully." })
                    : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? string.Empty;
                var result = await _loanService.RejectLoan(id, userId);
                return result
                    ? Ok(new { success = true, message = "Loan rejected successfully." })
                    : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var result = await _loanService.DeleteLoan(id);
            return result ? Ok() : NotFound();
        }

        [HttpPost("notify-creation")]
        public async Task<IActionResult> NotifyLoanCreation([FromQuery] string mobile, [FromQuery] string loanNo, [FromQuery] string amount)
        {
            bool result = await _messageService.SendLoanSmsAsync(mobile, loanNo, amount);

            if (result)
            {
                return Ok(new { success = true, message = "Loan notification SMS sent successfully." });
            }

            return BadRequest(new { success = false, message = "Failed to dispatch SMS via MSG91." });
        }
    }
}
