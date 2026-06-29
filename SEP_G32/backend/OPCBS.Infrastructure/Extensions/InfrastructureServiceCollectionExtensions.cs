using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OPCBS.Application.Interfaces.Repositories;
using OPCBS.Application.Interfaces.Services;
using OPCBS.Infrastructure.Persistence;
using OPCBS.Infrastructure.Repositories;
using OPCBS.Infrastructure.Services;

namespace OPCBS.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extension for Infrastructure layer
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext
        services.AddDbContext<OpcbsDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(OpcbsDbContext).Assembly.FullName)
            )
        );

        // Register Unit of Work and Repository
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register JWT Token Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register external service mock implementations (swap for real in production)
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<IFileStorageService, MockFileStorageService>();
        services.AddScoped<IPaymentService, MockPaymentService>();

        return services;
    }
}
