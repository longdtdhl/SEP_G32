using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OPCBS.Application.Extensions;
using OPCBS.Infrastructure.Extensions;
using OPCBS.Infrastructure.Persistence;
using OPCBS.Infrastructure.Seed;
using OPCBS.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Infrastructure services (DbContext, repositories, external services)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddInfrastructureServices(connectionString, builder.Configuration);

// Application services (business services, validators, AutoMapper)
builder.Services.AddApplicationServices();

// JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? "OPCBS-Super-Secret-Key-For-Development-Only-Minimum-32-Characters!";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "OPCBS",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "OPCBS",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // SignalR sends JWT token via query string (?access_token=...)
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                context.Fail("Invalid account identity.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<OpcbsDbContext>();
            var user = await db.Users.FindAsync([userId]);
            if (user == null || user.IsDeleted || user.Status != OPCBS.Domain.Enums.UserStatus.Active)
                context.Fail("Account is inactive or locked.");
        }
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyMethod().AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

// Swagger with JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OPCBS API",
        Version = "v1",
        Description = "Online Psychological Counseling Booking System API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Background Services
builder.Services.AddHostedService<OPCBS.Services.AppointmentReminderService>();

var app = builder.Build();

// Database deletion must be an explicit one-off action. Normal restarts preserve all data.
var resetDatabase = string.Equals(
    Environment.GetEnvironmentVariable("RESET_DB"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (resetDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<OpcbsDbContext>();
        context.Database.EnsureDeleted();
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OpcbsDbContext>();
    try
    {
        Console.WriteLine("Applying Schema Upgrades...");
        await OpcbsSchemaUpgrade.ApplyAdditiveUpgradesAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CRITICAL ERROR] Schema upgrades failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[INNER EXCEPTION]: {ex.InnerException.Message}");
    }

    try
    {
        Console.WriteLine("Applying Database Migrations...");
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARN] Database migrations step encountered: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[INNER EXCEPTION]: {ex.InnerException.Message}");
    }

    try
    {
        Console.WriteLine("Seeding Database...");
        await SeedData.SeedAsync(context);
        Console.WriteLine("Database initialization and seed complete!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CRITICAL ERROR] Database seeding failed: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[INNER EXCEPTION]: {ex.InnerException.Message}");
    }
}

// Đặt các dòng này ngay sau khi Build, trước các middleware khác
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OPCBS API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
