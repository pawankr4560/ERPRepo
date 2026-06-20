using FluentAssertions.Common;
using System.ComponentModel.DataAnnotations;
using Google.Apis.Util;
namespace ERPWebAppModels.Auth
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
    public class IdTokenRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }


    public class Clock :Google.Apis.Util.IClock
    {
        public DateTime Now => DateTime.Now.AddMinutes(10);

        public DateTime UtcNow => DateTime.UtcNow.AddMinutes(10);
    }
}
