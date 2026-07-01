using ERPWebAppService.Booking.Car;
using ERPWebAppService.Dashbord;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using System.Threading.RateLimiting;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Data.Repository;
using WebApp.Data.SeedData;
using WebApp.Service.Auth;
using WebApp.Service.Email;
using WebApp.Service.Message;
using WebApp.Service.Order;
using WebApp.Service.Product;
using WebApp.Service.Razorpay;
using WebApp.Service.Transaction;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.MaxDepth = 128;
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddEndpointsApiExplorer();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("EnableCORS", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials();
    });
});

// Database
var connectionString = configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Connection string 'Default' not found.");
}

builder.Services.AddDbContext<WebAppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["Redis:ConnectionString"];
});

// Seed Data
builder.Services.AddScoped<SeedData>();

// Http Context
builder.Services.AddHttpContextAccessor();

// AutoMapper
builder.Services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());

// MSG91 Http Client
builder.Services.AddHttpClient("MSG91Client", client =>
{
    var msg91 = configuration.GetSection("MSG91");

    var baseUrl = msg91["BaseUrl"];

    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }

    client.DefaultRequestHeaders.Add("authkey", msg91["AuthKey"] ?? "");
    client.DefaultRequestHeaders.Add("accept", "application/json");
});

builder.Services.AddHttpClient("AuthClient", client =>
{
    var issuer = configuration["Jwt:Issuer"];

    if (Uri.TryCreate(issuer, UriKind.Absolute, out var baseAddress))
    {
        client.BaseAddress = baseAddress;
    }
});

builder.Services.AddHttpClient("RazorpayClient", client =>
{
    client.BaseAddress = new Uri("https://api.razorpay.com/v1/");
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<WebAppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
    };
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Web App API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter JWT Token"
        });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Generic Repository
builder.Services.AddTransient<
    IGenericRepository<StripeCustomer>,
    GenericRepository<StripeCustomer>>();

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, WebApp.Service.Product.ProductService>();
builder.Services.AddScoped<IDashbordService, DashbordService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ILoanPaymentService, LoanPaymentService>();
builder.Services.AddScoped<ILoanDashboardService, LoanDashboardService>();
builder.Services.AddScoped<ILoanEMIScheduleService, LoanEMIScheduleService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingPaymentService, BookingPaymentService>();
builder.Services.AddScoped<IRazorpayService, RazorpayService>();

// Stripe Services
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ChargeService>();
builder.Services.AddScoped<Stripe.ProductService>();
builder.Services.AddScoped<PriceService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<CardService>();

builder.Services.AddHttpClient();
builder.Services.AddOptions();

// Stripe Configuration
var stripeSecretKey = configuration["Stripe:Secret_Key"];

if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    StripeConfiguration.ApiKey = stripeSecretKey;
}

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<SeedData>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database seeding failed");
    }
}

// Angular Static Files
app.UseDefaultFiles();
app.UseStaticFiles();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web App API V1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("EnableCORS");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Health Check Endpoint
app.MapGet("/test", () => "API Running");

// Angular Routing Support
app.MapFallbackToFile("/index.html");

app.Run();
