using Google.Apis.Auth;
using ERPWebAppModels.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApp.Model.Auth;
using WebApp.Model.Common;
using WebApp.Service.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Signup")]
        public async Task<IActionResult> Signup(SignupRequestModel model)
        {
            try
            {
                var result = await _authService.SignUpUser(model);
                return Ok(new ApiResponse(true, "User signup successfully. Please confirm your email before login.", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, ex));
            }
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                await _authService.ConfirmEmail(email, token);
                return Redirect(_authService.GetEmailConfirmationRedirectUrl(true));
            }
            catch
            {
                return Redirect(_authService.GetEmailConfirmationRedirectUrl(false));
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            try
            {
                var result = await _authService.Login(model);
                return Ok(new ApiResponse(true, "Login Successfully", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, ex.Message));
            }
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestModel model)
        {
            try
            {
                var result = await _authService.RefreshToken(model);
                return Ok(new ApiResponse(true, "Token refreshed successfully", result));
            }
            catch (Exception ex)
            {
                return Unauthorized(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginRequestModel model)
        {
            try
            {
                var clientId = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["GoogleAuth:ClientId"];

                var audience = string.IsNullOrWhiteSpace(clientId)
                    ? new[] { "154680420839-m4qrud76jiphfnvl905qipt6to24phvq.apps.googleusercontent.com" }
                    : new[] { clientId };

                var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, new GoogleJsonWebSignature.ValidationSettings()
                {
                    Clock = new Clock(),
                    Audience = audience
                });

                var result = await _authService.LoginWithGoogle(
                    payload.Email,
                    payload.GivenName ?? payload.Name ?? string.Empty,
                    payload.FamilyName ?? string.Empty,
                    payload.Subject);
                return Ok(new ApiResponse(true, "Google login successfully", result));
            }
            catch (InvalidJwtException ex)
            {
                return BadRequest(new { message = "Invalid Google token.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Unexpected error.", details = ex.Message });
            }
        }

        [HttpGet("VerifyToken")]
        public Task<IActionResult> VerifyGoogleToken(string idToken)
        {
            return GoogleLogin(new GoogleLoginRequestModel { IdToken = idToken });
        }

        [HttpGet("GetAddress")]
        public async Task<IActionResult> UserAddress(string address)
        {
            try
            {
                var result = await _authService.GetAddress(address);
                return Ok(new ApiResponse(true, null, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }

        [HttpGet("UserList")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var result = await _authService.UserList();
                return Ok(new ApiResponse(true, null, result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, null));
            }
        }
    }
}
