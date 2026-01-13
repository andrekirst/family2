using FamilyHub.Infrastructure.GraphQL.Extensions;
using FamilyHub.Infrastructure.Persistence.Extensions;
using FamilyHub.Modules.Family.Domain.Abstractions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.Modules.Family.Persistence.Repositories;
using FamilyHub.SharedKernel.Application.Behaviors;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Family;

/// <summary>
/// Dependency injection configuration for the Family module.
/// </summary>
/// <remarks>
/// <para>
/// Family module owns its own persistence layer with FamilyDbContext targeting the "family"
/// PostgreSQL schema. Repository implementations are in Family.Persistence.
/// </para>
/// <para>
/// Cross-module queries to Auth use IUserLookupService abstraction for proper
/// bounded context separation.
/// </para>
/// </remarks>
public static class FamilyModuleServiceRegistration
{
    /// <summary>
    /// Registers Family module services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFamilyModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register TimeProvider for timestamp management
        services.AddSingleton(TimeProvider.System);

        // PHASE 5: FamilyDbContext with pooled factory (performance optimization)
        // Uses same connection string as Auth but targets "family" schema
        services.AddPooledDbContextFactory<FamilyDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("FamilyHubDb");
            options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // Specify migrations assembly for EF Core tooling
                    npgsqlOptions.MigrationsAssembly(typeof(FamilyDbContext).Assembly.GetName().Name);
                })
                .UseSnakeCaseNamingConvention()
                .AddTimestampInterceptor(sp);
        });

        // Scoped DbContext from factory
        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<FamilyDbContext>>();
            return factory.CreateDbContext();
        });

        // Unit of Work for Family module
        services.AddScoped<IFamilyUnitOfWork, FamilyUnitOfWork>();

        // PHASE 5: Repository implementations now in Family module
        // These use FamilyDbContext and IUserLookupService for cross-module queries
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyMemberInvitationRepository, FamilyMemberInvitationRepository>();

        // Application Services (Anti-corruption layer for cross-module interactions)
        services.AddScoped<Application.Abstractions.IFamilyService, Application.Services.FamilyService>();

        // MediatR - Command/Query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(FamilyModuleServiceRegistration).Assembly);
            // Add pipeline behaviors
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // FluentValidation - Validators (placeholder - will be populated when validators are moved)
        // services.AddValidatorsFromAssembly(typeof(FamilyModuleServiceRegistration).Assembly);

        return services;
    }

    /// <summary>
    /// Registers Family module GraphQL type extensions (Queries and Mutations).
    /// Automatically discovers and registers all classes with [ExtendObjectType] attribute
    /// in the Family module assembly.
    /// </summary>
    /// <param name="builder">The GraphQL request executor builder.</param>
    /// <param name="loggerFactory">Optional logger factory for diagnostics.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method scans the Family module assembly for GraphQL type extensions and automatically
    /// registers them with Hot Chocolate. Type extensions include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Queries (classes decorated with [ExtendObjectType("Query")])</description></item>
    /// <item><description>Mutations (classes decorated with [ExtendObjectType("Mutation")])</description></item>
    /// </list>
    /// <para>
    /// <strong>Family module owns:</strong> FamilyType, FamilyQueries (via SharedKernel.IUserContext),
    /// FamilyMutations (CreateFamily), InviteFamilyMemberByEmail mutation.
    /// </para>
    /// <para>
    /// <strong>Auth module retains:</strong> FamilyTypeExtensions (requires User data for Members/Owner fields),
    /// AcceptInvitation and CancelInvitation mutations (modify User aggregate).
    /// Cross-module queries use IUserLookupService for proper bounded context separation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var loggerFactory = builder.Services.BuildServiceProvider().GetService&lt;ILoggerFactory&gt;();
    /// graphqlBuilder.AddFamilyModuleGraphQlTypes(loggerFactory);
    /// </code>
    /// </example>
    public static IRequestExecutorBuilder AddFamilyModuleGraphQlTypes(
        this IRequestExecutorBuilder builder,
        ILoggerFactory? loggerFactory = null)
    {
        return builder
            .RegisterDbContextFactory<Persistence.FamilyDbContext>() // Required for DataLoaders
            .AddType<Presentation.GraphQL.Types.FamilyType>() // Register FamilyType explicitly
            .AddTypeExtensionsFromAssemblies(
                [typeof(FamilyModuleServiceRegistration).Assembly],
                loggerFactory);
    }

    /// <summary>
    /// Registers Family module middleware in the ASP.NET Core pipeline.
    /// Currently a placeholder for future Family-specific middleware.
    ///
    /// Future middleware candidates:
    /// - Family context resolution (set current family from JWT or route)
    /// - Family permission validation middleware
    /// - Family-specific rate limiting
    ///
    /// EXECUTION ORDER:
    /// This method MUST be called AFTER UseAuthModule() because Family operations
    /// depend on authenticated user context being established.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseFamilyModule(this IApplicationBuilder app)
    {
        // Currently no Family-specific middleware
        // This extension point exists for future extensibility
        // Possible future additions:
        // - app.UseMiddleware<FamilyContextMiddleware>();
        // - app.UseMiddleware<FamilyPermissionMiddleware>();

        return app;
    }
}
