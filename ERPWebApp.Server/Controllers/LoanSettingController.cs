using global::WebApp.Data.Entity;
using global::WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Model.Payment;
using WebApp.Model.Transaction;

namespace WebApp.Server.Controllers
{
    namespace WebApp.Api.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public class LoanSettingController : ControllerBase
        {
            private readonly WebAppDbContext _dbContext;

            public LoanSettingController(WebAppDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            [HttpGet("interest-calculation-type")]
            public async Task<IActionResult> GetInterestCalculationType()
            {
                var setting = await _dbContext.LoanSetting
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    return Ok(new
                    {
                        interestCalculationType = "Flat"
                    });
                }

                return Ok(new
                {
                    interestCalculationType = setting.InterestCalculationType
                });
            }

            [HttpPut("interest-calculation-type")]
            public async Task<IActionResult> SaveInterestCalculationType(
                [FromBody] InterestCalculationTypeRequest model)
            {
                var setting = await _dbContext.LoanSetting
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    setting = new LoanSetting
                    {
                        InterestCalculationType = model.InterestCalculationType,
                        BookingPaymentFixedCharge = 0,
                        BookingPaymentPercentageCharge = 0,
                        UpdatedOn = DateTime.UtcNow
                    };

                    await _dbContext.LoanSetting.AddAsync(setting);
                }
                else
                {
                    setting.InterestCalculationType = model.InterestCalculationType;
                    setting.UpdatedOn = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    interestCalculationType = setting.InterestCalculationType
                });
            }

            [HttpGet("booking-payment-charges")]
            public async Task<IActionResult> GetBookingPaymentCharges()
            {
                var setting = await _dbContext.LoanSetting
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    fixedCharge = setting?.BookingPaymentFixedCharge ?? 0,
                    percentageCharge = setting?.BookingPaymentPercentageCharge ?? 0
                });
            }

            [HttpPut("booking-payment-charges")]
            public async Task<IActionResult> SaveBookingPaymentCharges(
                [FromBody] BookingPaymentChargeSettingRequest model)
            {
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var setting = await _dbContext.LoanSetting
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    setting = new LoanSetting
                    {
                        InterestCalculationType = true,
                        BookingPaymentFixedCharge = model.FixedCharge,
                        BookingPaymentPercentageCharge = model.PercentageCharge,
                        UpdatedOn = DateTime.UtcNow
                    };

                    await _dbContext.LoanSetting.AddAsync(setting);
                }
                else
                {
                    setting.BookingPaymentFixedCharge = model.FixedCharge;
                    setting.BookingPaymentPercentageCharge = model.PercentageCharge;
                    setting.UpdatedOn = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    fixedCharge = setting.BookingPaymentFixedCharge,
                    percentageCharge = setting.BookingPaymentPercentageCharge
                });
            }
        }
    }
}
