using System.Threading.Tasks;
using ERPWebAppModels.Profile;

namespace WebApp.Service.Profile
{
    public interface IProfileService
    {
        Task<ProfileDto> GetProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    }
}
