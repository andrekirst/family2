using FamilyHub.Infrastructure.GraphQL.Extensions;
using FamilyHub.Infrastructure.Persistence.Extensions;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Behaviors;
using FamilyHub.Modules.Auth.Application.Constants;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Behaviors;
using FamilyHub.SharedKernel.Interfaces;
using FamilyHub.Modules.Auth.Infrastructure.BackgroundServices;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Infrastructure.Messaging;
using FamilyHub.Modules.Auth.Infrastructure.Persistence;
using FamilyHub.Modules.Auth.Infrastructure.Services;
using FamilyHub.Modules.Auth.Persistence;
using FamilyHub.Modules.Auth.Persistence.Repositories;
using FluentValidation;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Authorization;
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

        // PHASE 4: Family repository registrations (implementation in Auth module, interface in Family module)
        // These implement Family module interfaces but remain in Auth module to avoid circular dependency
        // They use AuthDbContext (shared database) and will be moved in Phase 5+
        // Auth module registers them because they depend on AuthDbContext
        services.AddScoped<Family.Domain.Repositories.IFamilyRepository, FamilyRepository>();
        services.AddScoped<Family.Domain.Repositories.IFamilyMemberInvitationRepository, FamilyMemberInvitationRepository>();

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

        // RabbitMQ Publisher (stub for Phase 2)
        services.AddSingleton<IRabbitMqPublisher, StubRabbitMqPublisher>();

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
    /// This method scans the Auth module assembly for GraphQL type extensions and automatically
    /// registers them with Hot Chocolate. Type extensions include:
    /// - Queries (classes decorated with [ExtendObjectType("Query")])
    /// - Mutations (classes decorated with [ExtendObjectType("Mutation")])
    ///
    /// PHASE 4 UPDATE: Family GraphQL schema extracted to Family module.
    /// Moved to Family module:
    /// - FamilyType (core GraphQL type)
    /// - FamilyQueries (using SharedKernel.IUserContext)
    /// - FamilyMutations (CreateFamily - invokes Auth command)
    /// - InviteFamilyMemberByEmail mutation
    /// Auth module retains:
    /// - UserType, UserTypeExtensions
    /// - FamilyTypeExtensions (requires User data for Members/Owner - temporary)
    /// - AcceptInvitation, CancelInvitation mutations (modify User aggregate)
    /// TODO Phase 5+: Create IUserLookupService for proper abstraction
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
        return builder
            .RegisterDbContextFactory<AuthDbContext>()
            .AddType<Presentation.GraphQL.Types.UserType>() // Register UserType so extensions can be applied
            .AddTypeExtension<Presentation.GraphQL.Types.UserTypeExtensions>() // Explicitly register UserTypeExtensions
            .AddTypeExtension<Presentation.GraphQL.Types.FamilyTypeExtensions>() // Extends Family.Domain.Aggregates.FamilyAggregate (from Family module)
            .AddTypeExtensionsFromAssemblies(
                [typeof(AuthModuleServiceRegistration).Assembly],
                loggerFactory);
    }
}
