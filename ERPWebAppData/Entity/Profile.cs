using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class Profile : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
