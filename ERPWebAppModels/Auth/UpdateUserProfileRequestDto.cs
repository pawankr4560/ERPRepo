namespace ERPWebAppModels.Auth
{
    public class UpdateUserProfileRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? profileImageUrl { get; set; }
    }
}
