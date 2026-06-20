namespace WebApp.Model.Order
{
    public class CardResponseModel
    {
        public long ExpMonth { get; set; }
        public long ExpYear { get; set; }
        public string Last4 { get; set; }
        public bool IsDefault { get; set; }
        public string Id { get; set; }
        public string CustomerId { get; set; }
    }
}
