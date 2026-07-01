namespace ERPWebAppModels.Dashbord
{
    public class CreateNotificationRequestDto
    {
        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
