using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MyBookApp.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ValidToken = "token12345";

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Query["token"].ToString();

            if (token != ValidToken)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("Доступ запрещен. Неверный токен.");
            }
            else
            {
                await _next(context);
            }
        }
    }
}
