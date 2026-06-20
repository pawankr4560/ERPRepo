using WebApp.Model.Order;

public interface IStripeService
{
    Task<object> CreateCard();
    Task<string> CreateCheckout(long amount, string customerId, string email, string cardId);
    Task<string> CreateCustomer(string name, string email);
    Task<string> CreateProduct(double amount);
    Task<object> DeleteCard(string id);
    Task<object> GetCards();
    Task<object> SetDefaultCard(string cardId);
    Task<object> GetDefaultCard();
}