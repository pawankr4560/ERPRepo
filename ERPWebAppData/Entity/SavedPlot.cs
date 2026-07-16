using WebApp.Data.Entity;

namespace ERPWebAppData.Entity
{
    public class SavedPlot
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }

        public Guid PlotId { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        public Plot? Plot { get; set; }
    }
}
