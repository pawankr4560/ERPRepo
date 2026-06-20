using Microsoft.AspNetCore.Http;
using WebApp.Model.Constant;

namespace WebApp.Service.Middleware
{
    public class WebAppMiddleware
    {
        private readonly RequestDelegate _next;
        private string APIKEY = ConstantVarriable.ApiKey;

        public WebAppMiddleware(RequestDelegate next)
        {
            _next = next;
            this.APIKEY = ConstantVarriable.ApiKey;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(APIKEY, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(ErrorMessage.ApiKeyNotProvid);
                return;
            }
            await _next(context);
        }
    }
}
