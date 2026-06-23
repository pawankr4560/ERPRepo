using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;

namespace WebApp.Service.Auth;

public sealed class UserService : IUserService
{
    private readonly MongoDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(MongoDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public Task<User?> FindByEmailAsync(string email)
    {
        var normalized = Normalize(email);
        return _context.Users
            .Find(x => x.NormalizedEmail == normalized && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public Task<User?> FindByIdAsync(string id) =>
        _context.Users.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();

    public Task<List<User>> GetAllAsync() =>
        _context.Users.Find(x => !x.IsDeleted).ToListAsync();

    public async Task<User> CreateAsync(User user, string password)
    {
        user.Email = user.Email.Trim();
        user.UserName = string.IsNullOrWhiteSpace(user.UserName)
            ? user.Email
            : user.UserName.Trim();
        user.NormalizedEmail = Normalize(user.Email);
        user.NormalizedUserName = Normalize(user.UserName);

        if (await _context.Users.Find(x =>
                x.NormalizedEmail == user.NormalizedEmail
                || x.NormalizedUserName == user.NormalizedUserName).AnyAsync())
        {
            throw new InvalidOperationException("User already exists.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        await _context.Users.InsertOneAsync(user);
        return user;
    }

    public Task<bool> VerifyPasswordAsync(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return Task.FromResult(result != PasswordVerificationResult.Failed);
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedOn = DateTime.UtcNow;
        await _context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
