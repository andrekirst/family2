using FamilyHub.Infrastructure.GraphQL.Extensions;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Behaviors;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Infrastructure.Services;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Persistence.Repositories;
using FluentValidation;
using HotChocolate.Execution.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        services.AddScoped<IFamilyRepository, FamilyRepository>();

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

    /// <summary>
    /// Registers Auth module GraphQL type extensions (Queries and Mutations).
    /// Automatically discovers and registers all classes with [ExtendObjectType] attribute
    /// in the Auth module assembly.
    /// </summary>
    /// <param name="builder">The GraphQL request executor builder.</param>
    /// <param name="loggerFactory">Optional logger factory for diagnostics.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// This method scans the Auth module assembly for GraphQL type extensions and automatically
    /// registers them with Hot Chocolate. Type extensions include:
    /// - Queries (classes decorated with [ExtendObjectType("Query")])
    /// - Mutations (classes decorated with [ExtendObjectType("Mutation")])
    ///
    /// Example usage in Program.cs:
    /// <code>
    /// var loggerFactory = builder.Services.BuildServiceProvider().GetService&lt;ILoggerFactory&gt;();
    /// graphqlBuilder.AddAuthModuleGraphQLTypes(loggerFactory);
    /// </code>
    /// </remarks>
    public static IRequestExecutorBuilder AddAuthModuleGraphQlTypes(
        this IRequestExecutorBuilder builder,
        ILoggerFactory? loggerFactory = null)
    {
        return builder.AddTypeExtensionsFromAssemblies(
            [typeof(AuthModuleServiceRegistration).Assembly],
            loggerFactory);
    }
}
