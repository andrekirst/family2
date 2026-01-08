using FamilyHub.Infrastructure.GraphQL.Extensions;
using FamilyHub.Infrastructure.Persistence.Extensions;
using FamilyHub.SharedKernel.Application.Behaviors;
using HotChocolate.Execution.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Family;

/// <summary>
/// Dependency injection configuration for the Family module.
/// </summary>
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

        // PHASE 3: Persistence Layer
        // Repository implementations remain in Auth module to avoid circular dependency.
        // Repository interfaces (IFamilyRepository, IFamilyMemberInvitationRepository) are defined
        // in this module's Domain layer, but implementations are registered by Auth module.
        // This maintains logical separation while using shared AuthDbContext pragmatically.
        // TODO Phase 5+: Move repository implementations here when FamilyDbContext is introduced.

        // TODO: Register Application Services when implemented

        // MediatR - Command/Query handlers (placeholder - will be populated when handlers are moved)
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
    /// This method scans the Family module assembly for GraphQL type extensions and automatically
    /// registers them with Hot Chocolate. Type extensions include:
    /// - Queries (classes decorated with [ExtendObjectType("Query")])
    /// - Mutations (classes decorated with [ExtendObjectType("Mutation")])
    ///
    /// PHASE 4: Presentation Layer extraction (partial due to circular dependency constraints).
    /// Moved to Family module:
    /// - FamilyType (core GraphQL type)
    /// - InviteFamilyMemberByEmail mutation
    /// Remaining in Auth module (requires Auth context):
    /// - FamilyQueries (requires ICurrentUserService)
    /// - FamilyTypeExtensions (requires AuthDbContext, UserRepository)
    /// - Other invitation mutations that modify User aggregate
    /// TODO Phase 5+: Complete extraction with proper bounded context separation
    ///
    /// Example usage in Program.cs:
    /// <code>
    /// var loggerFactory = builder.Services.BuildServiceProvider().GetService&lt;ILoggerFactory&gt;();
    /// graphqlBuilder.AddFamilyModuleGraphQLTypes(loggerFactory);
    /// </code>
    /// </remarks>
    public static IRequestExecutorBuilder AddFamilyModuleGraphQlTypes(
        this IRequestExecutorBuilder builder,
        ILoggerFactory? loggerFactory = null)
    {
        return builder
            .AddType<Presentation.GraphQL.Types.FamilyType>() // Register FamilyType explicitly
            .AddTypeExtensionsFromAssemblies(
                [typeof(FamilyModuleServiceRegistration).Assembly],
                loggerFactory);
    }
}
