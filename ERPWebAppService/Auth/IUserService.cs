using WebApp.Data.Entity;

namespace WebApp.Service.Auth;

public interface IUserService
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(string id);
    Task<List<User>> GetAllAsync();
    Task<User> CreateAsync(User user, string password);
    Task<bool> VerifyPasswordAsync(User user, string password);
    Task UpdateAsync(User user);
}
