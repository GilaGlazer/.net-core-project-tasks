using System.Diagnostics;
using System.Net.Mail;

namespace webApiProject.Middlewares;

public class LogMiddleware
{
    private RequestDelegate next;

    public LogMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        //await c.Response.WriteAsync($"in Log Middleware- strat\n");
        // var timer = new Stopwatch();
        // timer.Start();
        // await next(c);
        // Console.WriteLine($"{c.Request.Path}.{c.Request.Method} took {timer.ElapsedMilliseconds} ms."
        //     + $" Success: {c.Items["success"]}"
        //     + $" User: {c.User?.FindFirst("userId")?.Value ?? "unknown"}");
        // await c.Response.WriteAsync("in Log Middleware- end\n");
        var timer = new Stopwatch();
        timer.Start();

        await next(context);

        var success = context.Items.ContainsKey("success") ? context.Items["success"] : "unknown";
        Console.WriteLine(
            $"{context.Request.Path}.{context.Request.Method} took {timer.ElapsedMilliseconds} ms."
                + $" Success: {success}"
                + $" User: {context.User?.FindFirst("userId")?.Value ?? "unknown"}"
        );
    }
}

public static class LogMiddlewareHelper
{
    public static void UseLog(this IApplicationBuilder a)
    {
        a.UseMiddleware<LogMiddleware>();
    }
}
