using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using webApiProject.Services;

namespace webApiProject.Middlewares
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;

        public UserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ActiveUserService activeUser)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                activeUser.UserId = int.TryParse(
                    context.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value,
                    out var userId
                )
                    ? userId
                    : -1;
                activeUser.Type =
                    context.User.Claims.FirstOrDefault(c => c.Type == "type")?.Value ?? "user";
            }

            await _next(context);
        }
    }
}
