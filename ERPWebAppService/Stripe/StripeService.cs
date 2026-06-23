using Microsoft.AspNetCore.Http;
using Stripe;
using MongoDB.Driver;
using WebApp.Data;
using WebApp.Data.Entity;
using WebApp.Model.Constant;
using WebApp.Model.Order;
using WebApp.Service.Auth;

public class StripeService : IStripeService
{
    private readonly IAuthService _authService;
    private readonly CustomerService _customerService;
    private readonly MongoDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Stripe.ProductService _productService;
    private readonly PriceService _priceService;
    private readonly Stripe.CardService _cardService;
    private readonly string userId;
    private readonly string customerId;
    private readonly string email;

    public StripeService(
        IAuthService authService,
        CustomerService customerService,
        MongoDbContext context,
        IHttpContextAccessor httpContextAccessor,
        Stripe.ProductService productService,
        PriceService priceService,
        Stripe.CardService cardService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _priceService = priceService ?? throw new ArgumentNullException(nameof(priceService));
        _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
        {
            throw new InvalidOperationException("HttpContext or User is null.");
        }

        var claims = httpContext.User.Claims.ToDictionary(c => c.Type, c => c.Value);
        userId = claims.TryGetValue("Id", out var id) ? id : throw new KeyNotFoundException("Id claim is missing.");
        customerId = claims.TryGetValue("CustomerId", out var custId) ? custId : throw new KeyNotFoundException("CustomerId claim is missing.");
        email = claims.TryGetValue("Email", out var emailClaim) ? emailClaim : throw new KeyNotFoundException("Email claim is missing.");
    }

    public async Task<string> CreateCustomer(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        if (string.IsNullOrEmpty(userId))
            throw new InvalidOperationException("User ID not found in claims.");
        try
        {
            var customerCreateOptions = new CustomerCreateOptions
            {
                Name = name,
                Email = email,
                Address = new AddressOptions
                {
                    Line1 = "443",
                    Line2 = "Sector 43",
                    City = "Mohali",
                    State = "Punjab",
                    PostalCode = "140601",
                    Country = "India"
                }
            };
            var result = await _customerService.CreateAsync(customerCreateOptions);
            var customer = new StripeCustomer
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsActive = true,
                IsDeleted = false,
                CustomerId = result.Id,
                CreatedOn = DateTime.UtcNow,
            };

            await _context.StripeCustomers.InsertOneAsync(customer);
            return result.Id;
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating customer.", ex);
        }
    }

    public async Task<string> CreateProduct(double amount)
    {
        try
        {
            var product = await _productService.CreateAsync(new ProductCreateOptions
            {
                Name = ConstantVarriable.ProductName,
            });
            var price = await _priceService.CreateAsync(new PriceCreateOptions
            {
                UnitAmount = (long)(amount * 100),
                Currency = "inr",
                Product = product.Id
            });
            return price.Id;
        }
        catch (Exception) { throw; }
    }

    public async Task<string> CreateCheckout(long amount, string customerId, string email, string cardId)
    {
        try
        {
            var request = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = ConstantVarriable.Currency,
                Customer = customerId,
                Description = ConstantVarriable.Description,
                ReceiptEmail = email,
                PaymentMethod = cardId,
                Confirm = true,
            };
            var payment = await new PaymentIntentService().CreateAsync(request);
            return await ConfirmPaymentIntentAsync(payment, cardId);
        }
        catch (Exception) { throw; }
    }

    public async Task<object> CreateCard()
    {
        try
        {
            var cardCreateOptions = new Stripe.CardCreateOptions
            {
                Source = "tok_visa"
            };
            return await _cardService.CreateAsync(customerId, cardCreateOptions);
        }
        catch (Exception) { throw; }
    }

    public async Task<object> DeleteCard(string id)
    {
        try
        {
            return await _cardService.DeleteAsync(customerId, id);
        }
        catch (Exception) { throw; }
    }

    public async Task<object> GetCards()
    {
        try
        {
            var customer = await _customerService.GetAsync(customerId);
            var defaultSourceId = customer.DefaultSourceId;
            var cards = await _cardService.ListAsync(customerId);
            var response = cards.Select(card => new CardResponseModel
            {
                ExpMonth = card.ExpMonth,
                ExpYear = card.ExpYear,
                Last4 = card.Last4,
                IsDefault = card.Id == defaultSourceId, 
                Id = card.Id,
                CustomerId = card.CustomerId
            }).ToList();

            return response;
        }
        catch (Exception) { throw; }
    }

    public async Task<object> GetDefaultCard()
    {
        try
        {
            return await _customerService.GetAsync(customerId);
        }
        catch (Exception) { throw; }
    }

    public async Task<object> SetDefaultCard(string cardId)
    {
        try
        {
            return await _customerService.UpdateAsync(customerId, new CustomerUpdateOptions
            {
                DefaultSource = cardId
            });
        }
        catch (Exception) { throw; }
    }

    private async Task<string> ConfirmPaymentIntentAsync(PaymentIntent paymentIntent, string paymentMethodId)
    {
        if (paymentIntent.Status == "requires_action" && paymentIntent.NextAction.Type == "use_stripe_sdk")
        {
            string callbackUrl = GetPaymentCallbackUrl();
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId,
                ReturnUrl = callbackUrl
            };
            var paymentIntentService = new PaymentIntentService();
            var confirmation = await paymentIntentService.ConfirmAsync(paymentIntent.Id, confirmOptions);
            return confirmation.NextAction.RedirectToUrl.Url;
        }
        else
        {
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId
            };
            var paymentIntentService = new PaymentIntentService();
            await paymentIntentService.ConfirmAsync(paymentIntent.Id, confirmOptions);
            return string.Empty;
        }
    }

    private string GetPaymentCallbackUrl()
    {
        //string localHost = _hostUrlSetting.AngularLocal;
        //if (_webHostEnvironment.IsProduction())
        //{
        //    localHost = _hostUrlSetting.Production;
        //}
        return $"{"kjashd"}/payment/message";
    }
}
