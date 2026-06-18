using System.Text;
using System.Text.Json.Serialization;
using AutoNex.Data;
using AutoNex.Enums;
using AutoNex.Helpers;
using AutoNex.Hubs;
using AutoNex.Middleware;
using AutoNex.Models;
using AutoNex.Services.Implementations;
using AutoNex.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("FixedWindow", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("AuthWindow", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IConsumableService, ConsumableService>();
builder.Services.AddScoped<IToolService, ToolService>();
builder.Services.AddScoped<IToolCategoryService, ToolCategoryService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IServiceOrderService, ServiceOrderService>();
builder.Services.AddScoped<IMileageAlertService, MileageAlertService>();
builder.Services.AddScoped<IFinancialRecordService, FinancialRecordService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IInventoryMovementService, InventoryMovementService>();
builder.Services.AddScoped<ITwilioService, TwilioService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SeedSettings>(builder.Configuration.GetSection("SeedData"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            var response = ApiResponse<object>.Fail("Error de validación", errors);
            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 128 * 1024;
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<MileageAlertBackgroundService>();
builder.Services.AddSingleton<IDashboardNotifier, DashboardNotifier>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    if (!context.Users.Any())
    {
        var seedSettings = scope.ServiceProvider.GetRequiredService<IOptions<SeedSettings>>();
        foreach (var seedUser in seedSettings.Value.Users)
        {
            var role = Enum.Parse<UserRole>(seedUser.Role);
            var user = new User
            {
                FullName = seedUser.FullName,
                Email = seedUser.Email.ToLowerInvariant().Trim(),
                PasswordHash = PasswordHelper.Hash(seedUser.Password),
                Role = role,
                IsActive = true
            };
            context.Users.Add(user);
        }
        await context.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseWebSockets();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    await next();
});

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.Run();
