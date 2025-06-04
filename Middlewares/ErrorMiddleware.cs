

namespace webApiProject.Middlewares;

public class ErrorMiddleware
{
    private RequestDelegate next;
    public ErrorMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Items["success"] = false;

        try
        {
            await next(context);
            context.Items["success"] = true;
        }
        catch (ApplicationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(ex.Message);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"An unexpected error occurred. Please contact support.");
        }
    }


}
public static class MiddlewareExtensions
{
    public static WebApplication UseErrorMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ErrorMiddleware>();
        return app;
    }
}