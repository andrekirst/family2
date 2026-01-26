using FamilyHub.Infrastructure.Persistence.Extensions;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Persistence;
using FamilyHub.Modules.UserProfile.Persistence.Repositories;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Mutations;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Queries;
using FamilyHub.SharedKernel.Application.Behaviors;
using FluentValidation;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile;

/// <summary>
/// Dependency injection configuration for the UserProfile module.
/// </summary>
/// <remarks>
/// <para>
/// UserProfile module owns its own persistence layer with UserProfileDbContext targeting the "user_profile"
/// PostgreSQL schema. Repository implementations will be added in subsequent issues.
/// </para>
/// <para>
/// Cross-module queries to Auth use IUserLookupService abstraction for proper
/// bounded context separation.
/// </para>
/// </remarks>
public static class UserProfileModuleServiceRegistration
{
    /// <summary>
    /// Registers UserProfile module services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserProfileModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // UserProfileDbContext with schema "user_profile"
        // Uses same connection string as other modules but targets "user_profile" schema
        services.AddDbContext<UserProfileDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("FamilyHubDb");
            options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // Specify migrations assembly for EF Core tooling
                    npgsqlOptions.MigrationsAssembly(typeof(UserProfileDbContext).Assembly.GetName().Name);
                })
                .UseSnakeCaseNamingConvention()
                .AddTimestampInterceptor(sp);
        });

        // Repositories
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();

        // MediatR - Command/Query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(UserProfileModuleServiceRegistration).Assembly);
            // Add pipeline behaviors
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            // ValidationBehavior - Validate commands using FluentValidation
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation - Validators
        services.AddValidatorsFromAssembly(typeof(UserProfileModuleServiceRegistration).Assembly);

        return services;
    }

    /// <summary>
    /// Registers UserProfile module GraphQL type extensions (Queries and Mutations).
    /// Automatically discovers and registers all classes with [ExtendObjectType] attribute
    /// in the UserProfile module assembly.
    /// </summary>
    /// <param name="builder">The GraphQL request executor builder.</param>
    /// <param name="loggerFactory">Optional logger factory for diagnostics.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method scans the UserProfile module assembly for GraphQL type extensions and automatically
    /// registers them with Hot Chocolate. Type extensions include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Queries (classes decorated with [ExtendObjectType("Query")])</description></item>
    /// <item><description>Mutations (classes decorated with [ExtendObjectType("Mutation")])</description></item>
    /// </list>
    /// <para>
    /// <strong>UserProfile module will own:</strong> UserProfileType, UserProfileQueries,
    /// UserProfileMutations (UpdateProfile, UpdateSettings, etc.)
    /// </para>
    /// </remarks>
    public static IRequestExecutorBuilder AddUserProfileModuleGraphQlTypes(
        this IRequestExecutorBuilder builder,
        ILoggerFactory? loggerFactory = null)
    {
        return builder
            .RegisterDbContextFactory<UserProfileDbContext>()
            .AddTypeExtension<UserProfileQueries>()
            .AddTypeExtension<UserProfileMutations>();
    }

    /// <summary>
    /// Registers UserProfile module middleware in the ASP.NET Core pipeline.
    /// Currently a placeholder for future UserProfile-specific middleware.
    ///
    /// Future middleware candidates:
    /// - Profile context resolution
    /// - Profile-specific rate limiting
    ///
    /// EXECUTION ORDER:
    /// This method should be called AFTER UseAuthModule() because UserProfile operations
    /// depend on authenticated user context being established.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseUserProfileModule(this IApplicationBuilder app)
    {
        // Currently no UserProfile-specific middleware
        // This extension point exists for future extensibility

        return app;
    }
}
