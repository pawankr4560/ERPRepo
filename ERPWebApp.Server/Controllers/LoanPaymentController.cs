using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Transaction;
using WebApp.Service.Transaction;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanPaymentController : ControllerBase
    {
        private readonly ILoanPaymentService _loanPaymentService;

        public LoanPaymentController(
            ILoanPaymentService loanPaymentService)
        {
            _loanPaymentService = loanPaymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _loanPaymentService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _loanPaymentService.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] LoanPaymentDto model)
        {
            var result = await _loanPaymentService.AddAsync(model);

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] LoanPaymentDto model)
        {
            var updated = await _loanPaymentService.UpdateAsync(id, model);

            if (!updated)
                return NotFound();

            return Ok("Loan Payment Updated Successfully");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _loanPaymentService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok("Loan Payment Deleted Successfully");
        }
    }
}
