using System.ComponentModel.DataAnnotations;

namespace WebApp.Model.Transaction
{
    public class LoanNotificationRequest
    {
        [Required]
        [Phone]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        public string LoanNo { get; set; } = string.Empty;

        [Required]
        public string Amount { get; set; } = string.Empty;
    }
}
