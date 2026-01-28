using FamilyHub.Infrastructure.GraphQL.Extensions;
using FamilyHub.Infrastructure.Persistence.Extensions;
using FamilyHub.Modules.Family.Domain.Abstractions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.Modules.Family.Persistence.Repositories;
using FamilyHub.SharedKernel.Application.Behaviors;
using FluentValidation;
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

        // PHASE 5: FamilyDbContext with domain event dispatching via IMediator
        // Uses same connection string as Auth but targets "family" schema
        services.AddDbContext<FamilyDbContext>((sp, options) =>
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

        // Unit of Work for Family module
        services.AddScoped<IFamilyUnitOfWork, FamilyUnitOfWork>();

        // PHASE 5: Repository implementations now in Family module
        // These use FamilyDbContext and IUserLookupService for cross-module queries
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyMemberInvitationRepository, FamilyMemberInvitationRepository>();
        services.AddScoped<IEmailOutboxRepository, EmailOutboxRepository>();

        // Application Services (Anti-corruption layer for cross-module interactions)
        services.AddScoped<Application.Abstractions.IFamilyService, Application.Services.FamilyService>();

        // Background services
        services.AddHostedService<Infrastructure.BackgroundServices.InvitationEmailService>();

        // MediatR - Command/Query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(FamilyModuleServiceRegistration).Assembly);
            // Add pipeline behaviors
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            // ValidationBehavior - Validate commands using FluentValidation
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation - Validators
        services.AddValidatorsFromAssembly(typeof(FamilyModuleServiceRegistration).Assembly);

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
            .RegisterDbContextFactory<FamilyDbContext>() // Required for DataLoaders
            .AddType<Presentation.GraphQL.Types.FamilyType>() // Register FamilyType explicitly
            .AddType<Presentation.GraphQL.Types.InvitationObjectType>() // Invitation Node type
            .AddType<Presentation.GraphQL.Types.InvitationStatusEnumType>() // Invitation status enum
            // NEW: Namespace container types for nested schema structure
            // mutation { family { inviteMemberByEmail, inviteMembers } }
            .AddType<Presentation.GraphQL.Namespaces.FamilyMutationsType>()
            // query { family { current, members } }
            .AddType<Presentation.GraphQL.Namespaces.FamilyQueriesType>()
            // NEW: Extensions that add mutations to namespace types
            .AddTypeExtension<Presentation.GraphQL.Namespaces.FamilyMutationsExtensions>()
            // NEW: Extensions that add queries to namespace types
            .AddTypeExtension<Presentation.GraphQL.Namespaces.FamilyQueriesExtensions>()
            // Query extensions - extend Query type with family-related queries (legacy)
            .AddTypeExtension<Presentation.GraphQL.Queries.FamilyQueries>()
            // Legacy mutation extensions - extend Mutation type directly (to be deprecated)
            // These will be removed once frontend is updated to use namespaced mutations
            .AddTypeExtension<Presentation.GraphQL.Mutations.InvitationMutations>();
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
