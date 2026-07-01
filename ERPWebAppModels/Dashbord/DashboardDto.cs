using ERPWebAppModels.Auth;

namespace ERPWebAppModels.Dashbord
{
    public class DashboardDto
    {
        public UserRes User { get; set; } = new();

        public int NotificationsCount { get; set; }

        public DashboardPreApprovedOfferDto? PreApprovedOffer { get; set; }

        public DashboardEmiSummaryDto? EmiSummary { get; set; }

        public List<DashboardActiveApplicationDto> ActiveApplications { get; set; } = new();
    }
    public class DashboardUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class DashboardPreApprovedOfferDto
    {
        public bool IsAvailable { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal InterestRate { get; set; }
        public string AmountLabel { get; set; } = string.Empty;
        public string RateLabel { get; set; } = string.Empty;
        public string OfferId { get; set; } = string.Empty;
    }

    public class DashboardEmiSummaryDto
    {
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string LoanId { get; set; } = string.Empty;
    }

    public class DashboardActiveApplicationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StepsCompleted { get; set; }
        public string ProgressLabel { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
    }
}
