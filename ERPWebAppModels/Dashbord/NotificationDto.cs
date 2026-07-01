namespace ERPWebAppModels.Dashbord
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
