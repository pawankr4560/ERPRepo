using System.ComponentModel.DataAnnotations;

namespace ERPWebAppModels.Auth
{
    public class GoogleLoginRequestModel
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
