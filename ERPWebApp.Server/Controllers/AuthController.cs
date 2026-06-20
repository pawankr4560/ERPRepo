using Google.Apis.Auth;
using ERPWebAppModels.Auth;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model.Auth;
using WebApp.Model.Common;
using WebApp.Service.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace WebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                return Ok(new ApiResponse(true, "User signup successfully", result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message, ex));
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


        [HttpGet("VerifyToken")]
        public async Task<IActionResult> VerifyGoogleToken(string idToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings()
                {
                    Clock = new Clock(),
                    Audience = new[] { "154680420839-m4qrud76jiphfnvl905qipt6to24phvq.apps.googleusercontent.com" }
                });


                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, payload.Email),
            new Claim("Email", payload.Email),
            new Claim("Id", payload.JwtId),
            new Claim("Name", payload.Name),
            new Claim("Role", "User")
        };

                var result = _authService.GetToken(claims);
                return Ok(new ApiResponse(true, null, result));
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
