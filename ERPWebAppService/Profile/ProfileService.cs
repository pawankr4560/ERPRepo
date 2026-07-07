using System;
using System.Threading.Tasks;
using WebApp.Data.Entity;
using ERPWebAppData.Entity;
using WebApp.Data.Repository;
using ERPWebAppModels.Profile;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Service.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepo;
        private readonly UserManager<User> _userManager;

        public ProfileService(IProfileRepository profileRepo, UserManager<User> userManager)
        {
            _profileRepo = profileRepo ?? throw new ArgumentNullException(nameof(profileRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<ProfileDto> GetProfileAsync(string userId)
        {
            var profile = await _profileRepo.GetByUserIdAsync(userId);
            if (profile == null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) throw new InvalidOperationException("User not found");

                profile = new ERPWebAppData.Entity.Profile
                {
                    UserId = userId,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    Phone = user.Phone.ToString(),
                    Address = user.Address ?? string.Empty,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };

                await _profileRepo.InsertAsync(profile);
                await _profileRepo.SaveAsync();

                profile = await _profileRepo.GetByUserIdAsync(userId);
            }

            return new ProfileDto
            {
                Name = profile!.Name,
                Phone = profile.Phone,
                Address = profile.Address,
                Email = profile.User?.Email ?? string.Empty
            };
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var profile = await _profileRepo.GetByUserIdAsync(userId);
            if (profile == null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return false;

                profile = new ERPWebAppData.Entity.Profile
                {
                    UserId = userId,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };
                profile.Name = dto.Name;
                profile.Phone = dto.Phone;
                profile.Address = dto.Address;
                await _profileRepo.InsertAsync(profile);
            }
            else
            {
                profile.Name = dto.Name;
                profile.Phone = dto.Phone;
                profile.Address = dto.Address;
                await _profileRepo.UpdateAsync(profile);
            }

            await _profileRepo.SaveAsync();
            return true;
        }
    }
}
