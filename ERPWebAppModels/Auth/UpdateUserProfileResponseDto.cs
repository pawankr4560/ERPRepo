namespace ERPWebAppModels.Auth
{
    public class UpdateUserProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long Mobile { get; set; }
        public string Address { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}
