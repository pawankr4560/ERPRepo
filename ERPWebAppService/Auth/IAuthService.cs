using ERPWebAppModels.Auth;
using System.Security.Claims;
using WebApp.Data.Entity;
using WebApp.Model.Auth;

namespace WebApp.Service.Auth
{
    public interface IAuthService
    {
        Task<bool> AuthenticateUser(string token);
        Task<bool> ConfirmEmail(string email, string token);
        Task<AuthTokenResponseModel> CreateAuthResponse(User user);
        Task<string> CreateToken(User user);
        string GetEmailConfirmationRedirectUrl(bool confirmed);
        Task<UserAddressResponseModel> GetAddress(string address);
        string GetToken(List<Claim> claims);
        Task<AuthTokenResponseModel> Login(LoginRequestModel model);
        Task<AuthTokenResponseModel> LoginWithGoogle(string email, string firstName, string lastName, string googleSubject);
        Task<AuthTokenResponseModel> RefreshToken(RefreshTokenRequestModel model);
        Task SendEmailFromSmtpAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task<bool> SignUpUser(SignupRequestModel model);
        Task<List<User>> UserList();
    }
}
