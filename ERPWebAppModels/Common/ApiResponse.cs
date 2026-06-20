namespace WebApp.Model.Common
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public object? Data { get; set; }

        public ApiResponse(bool isSuccess, string? message, object? data)
        {
            Success = isSuccess;
            if (isSuccess)
            {
                Message = message;
            }
            else
            {
                ErrorMessage = message;
            }
            Data = data;
        }
    }
}
