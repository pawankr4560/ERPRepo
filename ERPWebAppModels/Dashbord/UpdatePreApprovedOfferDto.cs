namespace ERPWebAppModels.Dashbord
{
    public class UpdatePreApprovedOfferDto
    {
        public int Id { get; set; }

        public decimal MaxAmount { get; set; }

        public decimal InterestRate { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public bool IsAvailable { get; set; }
    }
}
