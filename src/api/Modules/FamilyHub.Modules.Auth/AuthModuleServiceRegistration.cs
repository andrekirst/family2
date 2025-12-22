using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Behaviors;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Infrastructure.Services;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Modules.Auth;

/// <summary>
/// Dependency injection configuration for the Auth module.
/// </summary>
public static class AuthModuleServiceRegistration
{
    /// <summary>
    /// Registers Auth module services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("FamilyHubDb"))
                .UseSnakeCaseNamingConvention();
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Zitadel OAuth Configuration
        services.Configure<ZitadelSettings>(configuration.GetSection(ZitadelSettings.SectionName));

        // Infrastructure Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // HTTP Client for OAuth token exchange
        services.AddHttpClient();

        // HTTP Context Accessor for accessing HTTP context in services
        services.AddHttpContextAccessor();

        // MediatR - Command/Query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AuthModuleServiceRegistration).Assembly);
            // Add validation behavior to pipeline
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation - Validators
        services.AddValidatorsFromAssembly(typeof(AuthModuleServiceRegistration).Assembly);

        return services;
    }
}
