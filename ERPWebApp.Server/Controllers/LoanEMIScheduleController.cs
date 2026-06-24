using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Transaction;
using WebApp.Service.Transaction;

namespace WebApp.Server.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class LoanEMIScheduleController : ControllerBase
    {
        private readonly ILoanEMIScheduleService _loanEMIScheduleService;

        public LoanEMIScheduleController(
            ILoanEMIScheduleService loanEMIScheduleService)
        {
            _loanEMIScheduleService = loanEMIScheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _loanEMIScheduleService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _loanEMIScheduleService.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult> GetByLoanId(int loanId)
        {
            var result = await _loanEMIScheduleService.GetByLoanIdAsync(loanId);

            return Ok(result);
        }
        [HttpGet("loan-number/{loanNumber}")]
        public async Task<IActionResult> GetScheduleByLoanNumber(string loanNumber)
        {
            var result = await _loanEMIScheduleService
                .GetScheduleByLoanNumberAsync(loanNumber);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] LoanEMIScheduleDto model)
        {
            var result = await _loanEMIScheduleService.AddAsync(model);

            return Ok(result);
        }
        [HttpGet("unpaid-installments/{loanNumber}")]
        public async Task<IActionResult> GetUnpaidInstallments(string loanNumber)
        {
            var result = await _loanEMIScheduleService
                .GetUnpaidInstallmentsByLoanNumber(loanNumber);

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] LoanEMIScheduleDto model)
        {
            var updated = await _loanEMIScheduleService.UpdateAsync(id, model);

            if (!updated)
                return NotFound();

            return Ok("EMI Schedule Updated Successfully");
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _loanEMIScheduleService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok("EMI Schedule Deleted Successfully");
        }
    }
}
