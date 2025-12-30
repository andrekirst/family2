using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.SharedKernel.Presentation.GraphQL;

/// <summary>
/// Extension methods for IServiceCollection to register GraphQL infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Automatically registers all IPayloadFactory implementations from the specified assembly.
    /// Scans the assembly for concrete types implementing IPayloadFactory&lt;TResult, TPayload&gt;
    /// and registers them as scoped services in the DI container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assembly">Assembly to scan for payload factories</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPayloadFactoriesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        // Get all concrete types implementing IPayloadFactory<,>
        var payloadFactoryTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IPayloadFactory<,>)))
            .ToList();

        foreach (var factoryType in payloadFactoryTypes)
        {
            // Get the IPayloadFactory<TResult, TPayload> interface implemented by this type
            var payloadFactoryInterface = factoryType.GetInterfaces()
                .First(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IPayloadFactory<,>));

            // Register: services.AddScoped<IPayloadFactory<TResult, TPayload>, FactoryType>();
            services.AddScoped(payloadFactoryInterface, factoryType);
        }

        return services;
    }
}
