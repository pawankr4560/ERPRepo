using Microsoft.AspNetCore.Identity;

namespace WebApp.Data.Entity
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int LoginCount { get; set; }
        public long Phone { get; set; }
    }
}
