using WebApp.Data.Entity;
using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public interface IProfileRepository : IGenericRepository<Profile>
    {
        Task<Profile?> GetByUserIdAsync(string userId);
    }
}
