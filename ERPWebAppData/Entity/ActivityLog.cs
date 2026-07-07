using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class ActivityLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string HexColor { get; set; } = string.Empty;
    }
}
