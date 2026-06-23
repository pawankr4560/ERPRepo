using AutoMapper;
using ERPWebAppModels.Auth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Auth;

namespace WebApp.Service.Auth;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly MongoDbContext _context;
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserService userService,
        MongoDbContext context,
        IConfiguration configuration,
        IMapper mapper,
        HttpClient httpClient)
    {
        _userService = userService;
        _context = context;
        _configuration = configuration;
        _mapper = mapper;
        _httpClient = httpClient;
    }

    public async Task<bool> SignUpUser(SignupRequestModel model)
    {
        if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Password and confirm password do not match.");
        }

        var user = _mapper.Map<User>(model);
        user.CreatedOn = DateTime.UtcNow;
        user.IsActive = true;
        user.IsDeleted = false;
        user.UserName = model.Email;
        user.EmailConfirmed = true;
        user.Roles = ["User"];
        await _userService.CreateAsync(user, model.Password);
        return true;
    }

    public async Task<bool> AuthenticateUser(string token)
    {
        var response = await _httpClient.GetAsync($"auth/validate?token={token}");
        return response.IsSuccessStatusCode;
    }

    public async Task<string> Login(LoginRequestModel model)
    {
        var user = await _userService.FindByEmailAsync(model.Email)
            ?? throw new InvalidOperationException("User not exist.");

        if (!user.IsActive || !await _userService.VerifyPasswordAsync(user, model.Password))
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        return await CreateToken(user);
    }

    public async Task<string> CreateToken(User user)
    {
        var customer = await _context.StripeCustomers
            .Find(x => x.UserId == user.Id && !x.IsDeleted)
            .FirstOrDefaultAsync();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id),
            new("Email", user.Email),
            new("Id", user.Id),
            new("Phone", user.Phone.ToString()),
            new("FirstName", user.FirstName ?? string.Empty),
            new("LastName", user.LastName ?? string.Empty)
        };

        if (customer != null)
        {
            claims.Add(new Claim("CustomerId", customer.CustomerId));
        }

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        user.LoginCount++;
        user.LastLogin = DateTime.UtcNow;
        await _userService.UpdateAsync(user);
        return GetToken(claims);
    }

    public async Task<UserAddressResponseModel> GetAddress(string address)
    {
        var googleApiKey = _configuration["GooglePlace:api_key"];
        if (string.IsNullOrWhiteSpace(googleApiKey))
            throw new InvalidOperationException("Google Places API key is not configured.");

        var url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={Uri.EscapeDataString(address)}&key={Uri.EscapeDataString(googleApiKey)}&types=establishment";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Error calling Google Places API: {response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<UserAddressResponseModel>(json)
            ?? throw new InvalidOperationException("Google Places returned an empty response.");
    }

    public string GetToken(List<Claim> claims)
    {
        if (claims.Count == 0)
            throw new ArgumentException("Claims cannot be empty.", nameof(claims));

        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("JWT signing key is not configured.");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(10),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task<List<User>> UserList() => _userService.GetAllAsync();
}
