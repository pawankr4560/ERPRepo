namespace ERPWebAppModels.Dashboard
{
    public class DashboardSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal PendingEmi { get; set; }
        public int Bookings { get; set; }
        public int InventoryAlerts { get; set; }
    }
}
