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

        public AuthService(UserManager<User> userManager,
            SignInManager<User> signInManager,
            WebAppDbContext dbContext,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IMapper mapper,
            HttpClient httpClient)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _mapper = mapper;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SignUpUser(SignupRequestModel model)
        {
            try
            {
                if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
                {
                    throw new Exception("User is already exists.");
                }

                var user = _mapper.Map<User>(model);
                user.CreatedOn = DateTime.UtcNow;
                user.IsActive = true;
                user.IsDeleted = false;
                user.UserName = model.Email;
                user.EmailConfirmed = true;
                user.Id = Guid.NewGuid().ToString();
                var res = await _userManager.CreateAsync(user, model.Password);
                if (!res.Succeeded)
                {
                    throw new Exception(string.Join(" ", res.Errors.Select(error => error.Description)));
                }

                await _userManager.AddToRoleAsync(user, "User");
                await _userManager.GenerateEmailConfirmationTokenAsync(user);

                await _dbContext.UserDetails.AddAsync(new UserDetails
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Mobile = model.Phone,
                    Address = model.Address
                });
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> AuthenticateUser(string token)
        {
            var response = await _httpClient.GetAsync($"auth/validate?token={token}");
            return response.IsSuccessStatusCode;
        }

        public async Task<string> Login(LoginRequestModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email);

                if (user is null)
                    throw new Exception("User not exist.");

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (result.Succeeded)
                    return await CreateToken(user);
                throw new Exception("Invalid email or password.");
            }
            catch (Exception) { throw; }
        }

        public async Task<string> CreateToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var customer = await _dbContext.StripeCustomer.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
            var claims = new List<Claim>
            {
               new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
               new Claim("Email", user.Email),
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

            return GetToken(claims);
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
                    expires: DateTime.UtcNow.AddHours(10),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error generating JWT token.", ex);
            }
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
