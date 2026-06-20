using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Auth
{
    public class SignupRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]

        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]

        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public long Phone { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        public int Weight { get; set; }
        [Required]
        public int Height { get; set; }
        [Required]
        public double Calorie { get; set; }
    }
}
