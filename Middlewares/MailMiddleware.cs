using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace webApiProject.Middlewares;

public class MailMiddleware
{
    private readonly RequestDelegate next;
    private static bool isMailSent = false;

    public MailMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext c)
    {
        try
        {
            if (!isMailSent)
            {
                SendMail(new Exception("try to send mail"));
                isMailSent = true;
            }

            await next(c);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending mail: {ex.Message}");
            Console.WriteLine($"Error sending mail: {ex.Message}");
        }
    }

    private void SendMail(Exception ex)
    {
        var fromAddress = new MailAddress("r0583246798@gmail.com", "Error Notifier");
        var toAddress = new MailAddress("g0583247266@gmail.com", "admin");
        const string fromPassword = "pxri qwzo dxhn jkxk";
        string subject = "Application Error Notification";
        string body =
            $"An error occurred in the application:\n\n"
            + $"Message: {ex.Message}\n"
            + $"Time: {DateTime.Now}";

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = body,
        };
        smtp.Send(message);
    }
}

public static class MailMiddlewareHelper
{
    public static void UseMailMiddleware(this IApplicationBuilder a)
    {
        a.UseMiddleware<MailMiddleware>();
    }
}
