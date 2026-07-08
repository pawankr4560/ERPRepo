namespace ERPWebAppModels.Construction
{
    public class ConstructionDashboardDto
    {
        public int TotalProducts { get; set; }
        public int PendingQuotes { get; set; }
        public int ActiveOrders { get; set; }
        public int DeliveriesToday { get; set; }
        public List<ConstructionActivityDto> RecentActivities { get; set; }
            = new List<ConstructionActivityDto>();
    }
    public class ConstructionActivityDto
    {
        public string ActivityId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public ConstructionActivityDto(
            string activityId,
            string type,
            string title,
            string description,
            DateTime createdAt)
        {
            ActivityId = activityId;
            Type = type;
            Title = title;
            Description = description;
            CreatedAt = createdAt;
        }
    }
}
