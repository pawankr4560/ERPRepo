using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ERPWebAppData.Entity;
using WebApp.Data.Entity;

namespace WebApp.Data.SeedData
{
    public class SeedData
    {
        private readonly IConfiguration _configuration;
        private readonly WebAppDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public SeedData(IConfiguration configuration, WebAppDbContext dbContext, UserManager<User> userManager)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            _dbContext.Database.EnsureCreated();
            await SeedRoles();
            await SeedUsers();
            await SeedCarCategories();
            await SeedUnits();
        }

        public async Task SeedRoles()
        {
            var roles = await _dbContext.Roles.CountAsync();
            if (roles == 0)
            {
                _dbContext.Roles.Add(new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" });
                _dbContext.Roles.Add(new IdentityRole { Name = "User", NormalizedName = "USER" });
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task SeedUsers()
        {
            try
            {
                var user = await _userManager.FindByEmailAsync("admin@gmail.com");
                if (user == null)
                {
                    user = new User
                    {
                        Email = "admin@gmail.com",
                        UserName = "admin@gmail.com",
                        FirstName = "Admin",
                        LastName = "Admin",

                        EmailConfirmed = true,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(user, "Admin@123");
                    if (result == IdentityResult.Success)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ) { throw ; }
        }

        public async Task SeedCarCategories()
        {
            var defaultCategories = new[]
            {
                "Hatchback",
                "Sedan",
                "SUV",
                "MUV",
                "Luxury",
                "Electric"
            };

            var existingNames = await _dbContext.Categories
                .Select(category => category.Name)
                .ToListAsync();

            var categoriesToAdd = defaultCategories
                .Where(name => !existingNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                .Select(name => new Category { Name = name })
                .ToList();

            if (categoriesToAdd.Count == 0)
            {
                return;
            }

            _dbContext.Categories.AddRange(categoriesToAdd);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SeedUnits()
        {
            var defaultUnits = new[]
            {
                new UnitOfMeasure { UOMCode = "PCS", UOMName = "Pieces" },
                new UnitOfMeasure { UOMCode = "KG", UOMName = "Kilogram" },
                new UnitOfMeasure { UOMCode = "GM", UOMName = "Gram" },
                new UnitOfMeasure { UOMCode = "LTR", UOMName = "Litre" },
                new UnitOfMeasure { UOMCode = "ML", UOMName = "Millilitre" },
                new UnitOfMeasure { UOMCode = "BOX", UOMName = "Box" }
            };

            var existingCodes = await _dbContext.UnitOfMeasure
                .Select(unit => unit.UOMCode)
                .ToListAsync();

            var unitsToAdd = defaultUnits
                .Where(unit => !existingCodes.Contains(unit.UOMCode, StringComparer.OrdinalIgnoreCase))
                .Select(unit =>
                {
                    unit.CreatedOn = DateTime.UtcNow;
                    unit.CreatedBy = "System";
                    return unit;
                })
                .ToList();

            if (unitsToAdd.Count == 0)
            {
                return;
            }

            _dbContext.UnitOfMeasure.AddRange(unitsToAdd);
            await _dbContext.SaveChangesAsync();
        }

    }
}
