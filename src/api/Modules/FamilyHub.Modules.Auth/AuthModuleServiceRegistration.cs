using FamilyHub.Infrastructure.GraphQL.Extensions;
using FamilyHub.Infrastructure.Persistence.Extensions;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Behaviors;
using FamilyHub.Modules.Auth.Application.Constants;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Infrastructure.BackgroundServices;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Infrastructure.Persistence;
using FamilyHub.Modules.Auth.Infrastructure.Services;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Persistence.Repositories;
using FamilyHub.SharedKernel.Application.Behaviors;
using FamilyHub.SharedKernel.Interfaces;
using FluentValidation;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
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
        // Register TimeProvider for timestamp management
        services.AddSingleton(TimeProvider.System);

        // Register outbox interceptor
        services.AddSingleton<DomainEventOutboxInterceptor>();

        services.AddPooledDbContextFactory<AuthDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("FamilyHubDb");
            options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // CRITICAL: Specify migrations assembly explicitly for integration tests
                    // EF Core auto-discovery doesn't work when DbContext is created outside the main application
                    npgsqlOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.GetName().Name);
                })
                .UseSnakeCaseNamingConvention()
                .AddTimestampInterceptor(sp) // Add automatic timestamp management
                .AddInterceptors(sp.GetRequiredService<DomainEventOutboxInterceptor>()); // Add outbox pattern
        });

        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<AuthDbContext>>();
            return factory.CreateDbContext();
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories (Auth module)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

        // PHASE 5: IUserLookupService for cross-module queries
        // Family module uses this to query User data without direct DbContext access
        services.AddScoped<SharedKernel.Application.Abstractions.IUserLookupService, UserLookupService>();

        // PHASE 5: Family repository registrations REMOVED
        // Repository implementations moved to Family module with FamilyDbContext
        // See FamilyModuleServiceRegistration for new registrations

        // Zitadel OAuth Configuration
        services.Configure<ZitadelSettings>(configuration.GetSection(ZitadelSettings.SectionName));

        // Infrastructure Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // User Context - Scoped service holding authenticated user context for current request
        // Register both Auth-specific and SharedKernel IUserContext interfaces
        // Auth.IUserContext extends SharedKernel.IUserContext, so UserContextService implements both
        services.AddScoped<IUserContext, UserContextService>();
        services.AddScoped<SharedKernel.Application.Abstractions.IUserContext>(sp =>
            sp.GetRequiredService<IUserContext>());

        // Validation Cache - Scoped per HTTP request to eliminate duplicate database queries
        // between validators and handlers
        services.AddScoped<IValidationCache, ValidationCache>();

        // GraphQL Mutation Conventions (Hot Chocolate v14 native pattern)
        // No manual registration needed - mutations use [UseMutationConvention] attribute

        // HTTP Client for OAuth token exchange
        services.AddHttpClient();

        // HTTP Context Accessor for accessing HTTP context in services
        services.AddHttpContextAccessor();

        // Authorization: Custom requirements and handlers for role-based authorization
        // Roles are checked from IUserContext (populated by UserContextEnrichmentBehavior)
        services.AddScoped<IAuthorizationHandler, Infrastructure.Authorization.RequireOwnerHandler>();
        services.AddScoped<IAuthorizationHandler, Infrastructure.Authorization.RequireAdminHandler>();
        services.AddScoped<IAuthorizationHandler, Infrastructure.Authorization.RequireOwnerOrAdminHandler>();
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicyConstants.RequireOwner, policy =>
                policy.Requirements.Add(new Infrastructure.Authorization.RequireOwnerRequirement()))
            .AddPolicy(AuthorizationPolicyConstants.RequireAdmin, policy =>
                policy.Requirements.Add(new Infrastructure.Authorization.RequireAdminRequirement()))
            .AddPolicy(AuthorizationPolicyConstants.RequireOwnerOrAdmin, policy =>
                policy.Requirements.Add(new Infrastructure.Authorization.RequireOwnerOrAdminRequirement()));

        // MediatR - Command/Query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AuthModuleServiceRegistration).Assembly);
            // Add pipeline behaviors (order matters: Logging → UserContext → Authorization → Validation)
            // 1. LoggingBehavior - Log all requests/responses
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            // 2. UserContextEnrichmentBehavior - Load User aggregate from database
            cfg.AddOpenBehavior(typeof(UserContextEnrichmentBehavior<,>));
            // 3. AuthorizationBehavior - Check family context and role-based policies
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            // 4. ValidationBehavior - Validate input using FluentValidation
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation - Validators
        services.AddValidatorsFromAssembly(typeof(AuthModuleServiceRegistration).Assembly);

        // Note: RabbitMQ publisher is now registered centrally in Program.cs via AddRabbitMq()
        // This enables the real RabbitMQ implementation with retry policies and health checks

        // Background Services
        services.AddHostedService<OutboxEventPublisher>();

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
    /// <para>
    /// This method scans the Auth module assembly for GraphQL type extensions and automatically
    /// registers them with Hot Chocolate. Type extensions include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Queries (classes decorated with [ExtendObjectType("Query")])</description></item>
    /// <item><description>Mutations (classes decorated with [ExtendObjectType("Mutation")])</description></item>
    /// </list>
    /// <para>
    /// <strong>Auth module owns:</strong> UserType, UserTypeExtensions, FamilyTypeExtensions
    /// (requires User data for Members/Owner), AcceptInvitation and CancelInvitation mutations
    /// (modify User aggregate).
    /// </para>
    /// <para>
    /// <strong>Family module owns:</strong> FamilyType, FamilyQueries, FamilyMutations,
    /// InviteFamilyMemberByEmail mutation. Cross-module queries use IUserLookupService
    /// for proper bounded context separation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var loggerFactory = builder.Services.BuildServiceProvider().GetService&lt;ILoggerFactory&gt;();
    /// graphqlBuilder.AddAuthModuleGraphQlTypes(loggerFactory);
    /// </code>
    /// </example>
    public static IRequestExecutorBuilder AddAuthModuleGraphQlTypes(
        this IRequestExecutorBuilder builder,
        ILoggerFactory? loggerFactory = null)
    {
        return builder
            .RegisterDbContextFactory<AuthDbContext>()
            // Core entity types (must be registered before their extensions)
            .AddType<Presentation.GraphQL.Types.UserType>()
            .AddTypeExtension<Presentation.GraphQL.Types.UserTypeExtensions>()
            .AddTypeExtension<Presentation.GraphQL.Types.FamilyTypeExtensions>()
            // Namespace container types (no [ExtendObjectType] attribute - must be registered explicitly)
            // These provide GraphQL schema organization: query { auth { ... } invitations { ... } }
            .AddType<Presentation.GraphQL.Types.AuthType>()
            .AddType<Presentation.GraphQL.Types.InvitationsType>()
            // Auto-discover all type extensions with [ExtendObjectType] attribute
            // This includes: Query/Mutation extensions, AuthTypeExtensions, InvitationsTypeExtensions
            .AddTypeExtensionsFromAssemblies(
                [typeof(AuthModuleServiceRegistration).Assembly],
                loggerFactory);
    }

    /// <summary>
    /// Registers Auth module middleware in the ASP.NET Core pipeline.
    /// Currently includes:
    /// - PostgreSQL RLS context middleware (sets current_user_id for Row-Level Security)
    ///
    /// EXECUTION ORDER:
    /// This method MUST be called AFTER UseAuthentication() and UseAuthorization()
    /// because it relies on the authenticated user claims to set the RLS context.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app)
    {
        // PostgreSQL RLS context - sets session variable for Row-Level Security
        // Relies on authenticated user claims from preceding auth middleware
        app.UseMiddleware<Infrastructure.Middleware.PostgresRlsContextMiddleware>();

        return app;
    }
}
