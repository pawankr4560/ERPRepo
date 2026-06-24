using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Auth
{
    public class UserDetailsDto
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be a 10 digit number.")]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;
    }
}
