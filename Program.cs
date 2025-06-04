using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using webApiProject.Interfaces;
using webApiProject.Middlewares;
using webApiProject.Models;
using webApiProject.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/.log",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 100_000_000,
        retainedFileCountLimit: 30
    )
    .CreateLogger();

builder.Host.UseSerilog(); // שימוש בסרילוג כמנוע הלוגים

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;
        cfg.TokenValidationParameters = AuthTokenService.GetTokenValidationParameters();
    });

builder.Services.AddAuthorization(cfg =>
{
    cfg.AddPolicy("user", policy => policy.RequireClaim("type", "user", "admin"));
    cfg.AddPolicy("admin", policy => policy.RequireClaim("type", "admin"));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tasks", Version = "v1" });
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter JWT with Bearer into field",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
        }
    );
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});
builder.Services.AddActiveUserService();
builder.Services.AddUserService();
builder.Services.AddScoped<IService<Users>, UsersServiceJson>(); // רישום השירות `UsersServiceJson`
builder.Services.AddScoped<IService<Tasks>, ItemServiceJson<Tasks>>(); // רישום השירות `ItemServiceJson<Tasks>`

// הוספת Factory
builder.Services.AddScoped<Func<IService<Users>>>(sp =>
    () => sp.GetRequiredService<IService<Users>>()
);
builder.Services.AddScoped<Func<IService<Tasks>>>(sp =>
    () => sp.GetRequiredService<IService<Tasks>>()
);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    });
    app.MapScalarApiReference(options => options.WithTheme(ScalarTheme.Mars));
}

app.UseLog();
app.UseErrorMiddleware();

app.UseDefaultFiles(
    new DefaultFilesOptions { DefaultFileNames = new List<string> { "/html/item.html" } }
);

app.UseStaticFiles();

app.UseAuthentication();
app.UseMiddleware<UserMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
