using Microsoft.EntityFrameworkCore;
using WebApp.Data.Entity;
using ERPWebAppData.Entity;

namespace WebApp.Data.Repository
{
    public class ProfileRepository : GenericRepository<Profile>, IProfileRepository
    {
        private readonly WebAppDbContext _context;

        public ProfileRepository(WebAppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Profile?> GetByUserIdAsync(string userId)
        {
            return await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
