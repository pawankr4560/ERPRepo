namespace ERPWebAppModels.Dashbord
{
    public class CreatePreApprovedOfferRequestDto
    {
        public string UserId { get; set; } = string.Empty;

        public decimal MaxAmount { get; set; }

        public decimal InterestRate { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
