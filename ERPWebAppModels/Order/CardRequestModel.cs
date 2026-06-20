namespace WebApp.Model.Order
{
    public class CardRequestModel
    {
        public string Number { get; set; }
        public long Exp_month { get; set; }
        public long Exp_Year { get; set; }
        public string Cvc { get; set; }
        public string? Address_City { get; set; }
        public string? Address_Line1 { get; set; }
        public string? Address_Line2 { get; set; }
        public string? Address_State { get; set; }
        public string? Address_Country { get; set; }
        public string? Address_Zip { get; set; }
        public string? Name { get; set; }
    }
}
