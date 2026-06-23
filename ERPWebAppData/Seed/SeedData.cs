using ERPWebAppData.Entity;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using WebApp.Data.Entity;

namespace WebApp.Data.SeedData;

public sealed class SeedData
{
    private readonly MongoDbContext _context;
    private readonly IMongoSequenceService _sequences;
    private readonly IPasswordHasher<User> _passwordHasher;

    public SeedData(
        MongoDbContext context,
        IMongoSequenceService sequences,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _sequences = sequences;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
        await SeedCarCategoriesAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in new[] { "Admin", "User" })
        {
            var normalized = roleName.ToUpperInvariant();
            if (!await _context.Roles.Find(x => x.NormalizedName == normalized).AnyAsync())
            {
                await _context.Roles.InsertOneAsync(new Role
                {
                    Name = roleName,
                    NormalizedName = normalized
                });
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        const string email = "admin@gmail.com";
        var normalizedEmail = email.ToUpperInvariant();
        if (await _context.Users.Find(x => x.NormalizedEmail == normalizedEmail).AnyAsync())
        {
            return;
        }

        var user = new User
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            UserName = email,
            NormalizedUserName = normalizedEmail,
            FirstName = "Admin",
            LastName = "Admin",
            EmailConfirmed = true,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            Roles = ["Admin"]
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, "Admin@123");
        await _context.Users.InsertOneAsync(user);
    }

    private async Task SeedCarCategoriesAsync()
    {
        var defaultCategories = new[] { "Hatchback", "Sedan", "SUV", "MUV", "Luxury", "Electric" };
        var existingNames = await _context.Categories
            .Find(Builders<Category>.Filter.Empty)
            .Project(x => x.Name)
            .ToListAsync();

        foreach (var name in defaultCategories.Where(
                     name => !existingNames.Contains(name, StringComparer.OrdinalIgnoreCase)))
        {
            await _context.Categories.InsertOneAsync(new Category
            {
                Id = await _sequences.GetNextAsync("Categories"),
                Name = name
            });
        }
    }
}
