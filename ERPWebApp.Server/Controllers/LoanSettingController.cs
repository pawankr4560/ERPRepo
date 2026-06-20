using global::WebApp.Data.Entity;
using global::WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Model.Transaction;

namespace WebApp.Server.Controllers
{
    namespace WebApp.Api.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
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
        }
    }
}
