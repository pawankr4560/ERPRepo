using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using WebApp.Data;
using WebApp.Data.Entity;
using ERPWebAppModels.Auth;
using WebApp.Model.Auth;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace WebApp.Service.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly WebAppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserManager<User> userManager,
            SignInManager<User> signInManager,
            WebAppDbContext dbContext,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IMapper mapper,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient("AuthClient");
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> SignUpUser(SignupRequestModel model)
        {
            try
            {
                if (model.Password != model.ConfirmPassword)
                {
                    throw new Exception("Password and confirm password do not match.");
                }

                if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
                {
                    throw new Exception("User is already exists.");
                }

                var user = _mapper.Map<User>(model);
                user.CreatedOn = DateTime.UtcNow;
                user.IsActive = true;
                user.IsDeleted = false;
                user.UserName = model.Email;
                user.EmailConfirmed = false;
                user.Id = Guid.NewGuid().ToString();
                var res = await _userManager.CreateAsync(user, model.Password);
                if (!res.Succeeded)
                {
                    throw new Exception(string.Join(" ", res.Errors.Select(error => error.Description)));
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    throw new Exception(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = BuildEmailConfirmationLink(user.Email!, emailConfirmationToken);
                var emailBody = BuildSignupEmailBody(user, confirmationLink);
                await SendEmailFromSmtpAsync(user.Email!, "Welcome to ERP Web App", emailBody);
                return true;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> ConfirmEmail(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Email confirmation token is required.", nameof(token));

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                throw new Exception("User not found.");

            if (user.EmailConfirmed)
                return true;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return true;

            throw new Exception(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        public async Task SendEmailFromSmtpAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));

            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = _configuration["Smtp:Port"];
            var smtpUserName = _configuration["Smtp:UserName"];
            var smtpPassword = _configuration["Smtp:Password"];
            var smtpFromEmail = _configuration["Smtp:FromEmail"];
            var smtpFromName = _configuration["Smtp:FromName"];
            var smtpEnableSsl = _configuration["Smtp:EnableSsl"];

            if (string.IsNullOrWhiteSpace(smtpHost))
                throw new InvalidOperationException("SMTP host is not configured.");

            if (!int.TryParse(smtpPort, out var port))
                throw new InvalidOperationException("SMTP port is not configured correctly.");

            if (string.IsNullOrWhiteSpace(smtpFromEmail))
                throw new InvalidOperationException("SMTP from email is not configured.");

            bool.TryParse(smtpEnableSsl, out var enableSsl);

            using var message = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(smtpFromName)
                    ? new MailAddress(smtpFromEmail)
                    : new MailAddress(smtpFromEmail, smtpFromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(new MailAddress(toEmail));

            using var smtpClient = new SmtpClient(smtpHost, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(smtpUserName))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
            }
            else
            {
                smtpClient.UseDefaultCredentials = true;
            }

            await smtpClient.SendMailAsync(message);
        }

        private string BuildEmailConfirmationLink(string email, string token)
        {
            var baseUrl = _configuration["EmailConfirmation:BaseUrl"];
            var request = _httpContextAccessor.HttpContext?.Request;

            if (string.IsNullOrWhiteSpace(baseUrl) && request is not null)
            {
                baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = _configuration["Jwt:Issuer"];
            }

            if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                throw new InvalidOperationException("Email confirmation base URL is not configured.");
            }

            var confirmPath = "api/Auth/ConfirmEmail";
            var builder = new UriBuilder(new Uri(baseUri, confirmPath))
            {
                Query = $"email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}"
            };

            return builder.Uri.ToString();
        }

        public string GetEmailConfirmationRedirectUrl(bool confirmed)
        {
            var configuredUrl = confirmed
                ? _configuration["EmailConfirmation:SuccessRedirectUrl"]
                : _configuration["EmailConfirmation:FailureRedirectUrl"];

            if (!string.IsNullOrWhiteSpace(configuredUrl))
                return configuredUrl;

            return confirmed
                ? "/auth/login?emailConfirmed=true"
                : "/auth/login?emailConfirmed=false";
        }

        private static string BuildSignupEmailBody(User user, string confirmationLink)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            var greetingName = string.IsNullOrWhiteSpace(fullName) ? user.Email : fullName;
            var encodedConfirmationLink = WebUtility.HtmlEncode(confirmationLink);

            return $@"
                <p>Hello {WebUtility.HtmlEncode(greetingName)},</p>
                <p>Your ERP Web App account has been created successfully.</p>
                <p>Please confirm your email address to activate your account.</p>
                <p><a href=""{encodedConfirmationLink}"">Confirm email address</a></p>
                <p>If the button does not work, copy and paste this link into your browser:</p>
                <p>{encodedConfirmationLink}</p>
                <p>Thank you,<br/>ERP Web App</p>";
        }

        public async Task<bool> AuthenticateUser(string token)
        {
            var response = await _httpClient.GetAsync($"auth/validate?token={token}");
            return response.IsSuccessStatusCode;
        }

        public async Task<AuthTokenResponseModel> Login(LoginRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email);

                if (user is null)
                    throw new Exception("User not exist.");

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (result.Succeeded)
                    return await CreateAuthResponse(user);
                throw new Exception("Invalid email or password.");
            }
            catch (Exception) { throw; }
        }

        public async Task<AuthTokenResponseModel> LoginWithGoogle(string email, string firstName, string lastName, string googleSubject)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Google email is required.", nameof(email));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new Exception(string.Join(" ", createResult.Errors.Select(error => error.Description)));

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                    throw new Exception(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
            }
            else if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            var existingLogin = await _userManager.FindByLoginAsync("Google", googleSubject);
            if (existingLogin == null)
            {
                var loginResult = await _userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo("Google", googleSubject, "Google"));

                if (!loginResult.Succeeded)
                    throw new Exception(string.Join(" ", loginResult.Errors.Select(error => error.Description)));
            }

            return await CreateAuthResponse(user);
        }

        public async Task<AuthTokenResponseModel> RefreshToken(RefreshTokenRequestModel model)
        {
            var principal = GetPrincipalFromExpiredToken(model.AccessToken);
            var userId = principal.FindFirst("Id")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new SecurityTokenException("Invalid access token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted || !user.IsActive)
                throw new SecurityTokenException("Invalid user.");

            var storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, "ERPWebApp", "RefreshToken");
            var storedRefreshTokenExpiry = await _userManager.GetAuthenticationTokenAsync(user, "ERPWebApp", "RefreshTokenExpiresUtc");

            if (string.IsNullOrWhiteSpace(storedRefreshToken) ||
                storedRefreshToken != model.RefreshToken ||
                !DateTime.TryParse(storedRefreshTokenExpiry, out var refreshTokenExpiresAtUtc) ||
                refreshTokenExpiresAtUtc <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token.");
            }

            return await CreateAuthResponse(user);
        }

        public async Task<AuthTokenResponseModel> CreateAuthResponse(User user)
        {
            var accessTokenExpiresAtUtc = DateTime.UtcNow.AddHours(10);
            var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7);
            var accessToken = await CreateToken(user, accessTokenExpiresAtUtc);
            var refreshToken = GenerateRefreshToken();

            await _userManager.SetAuthenticationTokenAsync(user, "ERPWebApp", "RefreshToken", refreshToken);
            await _userManager.SetAuthenticationTokenAsync(
                user,
                "ERPWebApp",
                "RefreshTokenExpiresUtc",
                refreshTokenExpiresAtUtc.ToString("O"));

            return new AuthTokenResponseModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
            };
        }

        public async Task<string> CreateToken(User user)
        {
            return await CreateToken(user, DateTime.UtcNow.AddHours(10));
        }

        private async Task<string> CreateToken(User user, DateTime expiresAtUtc)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var customer = await _dbContext.StripeCustomer.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
            var claims = new List<Claim>
            {
               new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
               new Claim("Email", user.Email ?? string.Empty),
               new Claim("Id", user.Id),
               new Claim("Phone", user.Phone.ToString()??string.Empty),
               new Claim("FirstName", user.FirstName ?? string.Empty),
               new Claim("LastName", user.LastName ?? string.Empty),
            };

            if (customer != null)
            {
                claims.Add(new Claim("CustomerId", customer.CustomerId));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            user.LoginCount += 1;
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return GetToken(claims, expiresAtUtc);
        }

        public async Task<UserAddressResponseModel> GetAddress(string address)
        {
            var googleApiKey = _configuration["GooglePlace:api_key"];
            if (string.IsNullOrWhiteSpace(googleApiKey))
                throw new InvalidOperationException("Google Places API key is not configured.");

            string url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={Uri.EscapeDataString(address)}&key={Uri.EscapeDataString(googleApiKey)}&types=establishment";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error calling Google Places API: {response.StatusCode}");

            var res = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserAddressResponseModel>(res)
                ?? throw new InvalidOperationException("Google Places returned an empty response.");
        }

        public string GetToken(List<Claim> claims)
        {
            return GetToken(claims, DateTime.UtcNow.AddHours(10));
        }

        private string GetToken(List<Claim> claims, DateTime expiresAtUtc)
        {
            if (claims == null || !claims.Any())
                throw new ArgumentException("Claims cannot be null or empty.", nameof(claims));

            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(jwtKey))
                    throw new InvalidOperationException("JWT signing key is not configured.");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expiresAtUtc,
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating JWT token.", ex);
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("JWT signing key is not configured.");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid access token.");
            }

            return principal;
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<List<User>> UserList()
        {
            try
            {
                return await _dbContext.Users.ToListAsync();
            }
            catch (Exception) { throw; }
        }

    }
}
