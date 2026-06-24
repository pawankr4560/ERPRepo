using ERPWebAppModels.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Entity;

namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDetailsController : ControllerBase
    {
        private readonly WebAppDbContext _dbContext;

        public UserDetailsController(WebAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _dbContext.UserDetails
                .AsNoTracking()
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(x => new UserDetailsDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Mobile = x.Mobile.ToString(),
                    Address = x.Address
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDetailsDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mobileText = model.Mobile?.Trim() ?? string.Empty;
            if (mobileText.Length != 10 || mobileText.Any(c => !char.IsDigit(c)))
                return BadRequest(new { message = "Mobile must be a 10 digit number." });

            var mobile = long.Parse(mobileText);
            var mobileExists = await _dbContext.UserDetails
                .AsNoTracking()
                .AnyAsync(x => x.Mobile == mobile);
            if (mobileExists)
                return Conflict(new { message = "Mobile number already exists." });

            var user = new UserDetails
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Mobile = mobile,
                Address = model.Address.Trim()
            };

            await _dbContext.UserDetails.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            model.Id = user.Id;
            return Ok(model);
        }
    }
}
