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

    }
}
