using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Transaction;

namespace WebApp.Server.WebApp.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoanSettingController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;

    public LoanSettingController(MongoDbContext context, IMongoSequenceService sequences)
    {
        _context = context;
        _sequences = sequences;
    }

    [HttpGet("interest-calculation-type")]
    public async Task<IActionResult> GetInterestCalculationType()
    {
        var setting = await _context.LoanSettings
            .Find(Builders<LoanSetting>.Filter.Empty).FirstOrDefaultAsync();
        return Ok(new
        {
            interestCalculationType = setting?.InterestCalculationType ?? false
        });
    }

    [HttpPut("interest-calculation-type")]
    public async Task<IActionResult> SaveInterestCalculationType(
        [FromBody] InterestCalculationTypeRequest model)
    {
        var setting = await _context.LoanSettings
            .Find(Builders<LoanSetting>.Filter.Empty).FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new LoanSetting
            {
                Id = await _sequences.GetNextAsync("LoanSettings"),
                InterestCalculationType = model.InterestCalculationType,
                UpdatedOn = DateTime.UtcNow
            };
            await _context.LoanSettings.InsertOneAsync(setting);
        }
        else
        {
            setting.InterestCalculationType = model.InterestCalculationType;
            setting.UpdatedOn = DateTime.UtcNow;
            await _context.LoanSettings.ReplaceOneAsync(x => x.Id == setting.Id, setting);
        }
        return Ok(new { interestCalculationType = setting.InterestCalculationType });
    }
}
