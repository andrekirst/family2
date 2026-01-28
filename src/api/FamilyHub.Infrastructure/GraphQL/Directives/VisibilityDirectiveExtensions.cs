using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure.GraphQL.Directives;

/// <summary>
/// Extension methods for registering the @visible directive and related services.
/// </summary>
public static class VisibilityDirectiveExtensions
{
    /// <summary>
    /// Adds the @visible directive and visibility context services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVisibilityDirectiveServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IVisibilityContext, VisibilityContext>();
        return services;
    }

    /// <summary>
    /// Adds the @visible directive type to the GraphQL schema.
    /// </summary>
    /// <param name="builder">The GraphQL request executor builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// <para>
    /// This method registers:
    /// <list type="bullet">
    /// <item><description>The VisibleDirectiveType directive definition</description></item>
    /// <item><description>The FieldVisibility enum type</description></item>
    /// <item><description>The VisibilityFieldMiddleware for runtime enforcement</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// builder.Services
    ///     .AddGraphQLServer()
    ///     .AddVisibleDirective()
    ///     .AddQueryType&lt;Query&gt;();
    /// </code>
    /// </para>
    /// </remarks>
    public static IRequestExecutorBuilder AddVisibleDirective(this IRequestExecutorBuilder builder)
    {
        return builder
            .AddDirectiveType<VisibleDirectiveType>()
            .UseField<VisibilityFieldMiddleware>();
    }
}
