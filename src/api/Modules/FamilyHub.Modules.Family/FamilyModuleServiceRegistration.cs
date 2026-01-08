using FamilyHub.Infrastructure.GraphQL.Extensions;
using FamilyHub.Infrastructure.Persistence.Extensions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Persistence;
using FamilyHub.Modules.Family.Persistence.Repositories;
using FamilyHub.SharedKernel.Application.Behaviors;
using HotChocolate.Execution.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Register DbContext (using "auth" schema for Phase 0 safety strategy)
        services.AddPooledDbContextFactory<FamilyDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("FamilyHubDb");
            options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    // CRITICAL: Specify migrations assembly explicitly for integration tests
                    npgsqlOptions.MigrationsAssembly(typeof(FamilyDbContext).Assembly.GetName().Name);
                })
                .UseSnakeCaseNamingConvention()
                .AddTimestampInterceptor(sp);
        });

        services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<FamilyDbContext>>();
            return factory.CreateDbContext();
        });

        // Repositories
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyMemberInvitationRepository, FamilyMemberInvitationRepository>();

        // MediatR - Command/Query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(FamilyModuleServiceRegistration).Assembly);
            // Add pipeline behaviors
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // FluentValidation - Validators
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
            .RegisterDbContextFactory<FamilyDbContext>()
            .AddTypeExtensionsFromAssemblies(
                [typeof(FamilyModuleServiceRegistration).Assembly],
                loggerFactory);
    }
}
