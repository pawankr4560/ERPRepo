using ERPWebAppModels.Auth;
using System.Security.Claims;
using WebApp.Data.Entity;
using WebApp.Model.Auth;

namespace WebApp.Service.Auth
{
    public interface IAuthService
    {
        Task<bool> AuthenticateUser(string token);
        Task<string> CreateToken(User user);
        Task<UserAddressResponseModel> GetAddress(string address);
        string GetToken(List<Claim> claims);
        Task<string> Login(LoginRequestModel model);
        Task<bool> SignUpUser(SignupRequestModel model);
        Task<List<User>> UserList();
    }
}