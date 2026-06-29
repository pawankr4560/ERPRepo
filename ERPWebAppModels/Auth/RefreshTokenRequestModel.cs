using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Auth
{
    public class RefreshTokenRequestModel
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
