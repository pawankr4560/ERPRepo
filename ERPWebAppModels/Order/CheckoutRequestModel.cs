namespace WebApp.Model.Order
{
    public class CheckoutRequestModel
    {
        public long Amount { get; set; }    
        public string CardId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
    }
}
