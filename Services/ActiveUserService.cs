using System;
using webApiProject.Services;

namespace webApiProject.Services
{
    public class ActiveUserService
    {
        public int UserId { get; set; } = -1;
        public string Type { get; set; } = "user";
    }
}

public static partial class ServiceUlilities
{
    public static IServiceCollection AddActiveUserService(this IServiceCollection services)
    {
        services.AddScoped<ActiveUserService>();
        return services;
    }
}
