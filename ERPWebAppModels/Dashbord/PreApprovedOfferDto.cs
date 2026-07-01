namespace ERPWebAppModels.Dashbord
{
    public class PreApprovedOfferDto
    {
        public int Id { get; set; }

        public string OfferCode { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public decimal MaxAmount { get; set; }

        public decimal InterestRate { get; set; }

        public string AmountLabel =>
            $"Up to ₹{MaxAmount:N0}";

        public string RateLabel =>
            $"Interest rate from {InterestRate}% p.a.";
    }
}
