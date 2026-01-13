using System.Reflection;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Infrastructure.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilyHub.SharedKernel.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering specification services in the DI container.
/// </summary>
public static class SpecificationServiceCollectionExtensions
{
    /// <summary>
    /// Registers all specifications from the specified assemblies via assembly scanning.
    /// Specifications with constructor dependencies are registered as transient services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for specifications.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSpecifications(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (assemblies.Length == 0)
        {
            return services;
        }

        foreach (var assembly in assemblies)
        {
            RegisterSpecificationsFromAssembly(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers specification diagnostics subscriber for logging.
    /// Call ActivateSpecificationDiagnostics() after service provider is built.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSpecificationDiagnostics(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<SpecificationDiagnosticSubscriber>();

        return services;
    }

    /// <summary>
    /// Activates specification diagnostics after service provider is built.
    /// Call this in application startup after building the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>An IDisposable to deactivate diagnostics on shutdown.</returns>
    public static IDisposable ActivateSpecificationDiagnostics(this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var subscriber = serviceProvider.GetRequiredService<SpecificationDiagnosticSubscriber>();
        var logger = serviceProvider.GetRequiredService<ILogger<SpecificationDiagnosticSubscriber>>();

        var subscription = subscriber.Activate();
        logger.LogDiagnosticsActivated();

        return subscription;
    }

    private static void RegisterSpecificationsFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var specificationTypes = assembly.GetTypes()
            .Where(IsConcreteSpecification)
            .Where(HasConstructorDependencies)
            .ToList();

        foreach (var specType in specificationTypes)
        {
            // Register as transient - specifications are cheap and should be fresh
            services.AddTransient(specType);

            // Also register by interface for specifications that implement a specific interface
            var specificationInterface = specType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ISpecification<>));

            if (specificationInterface is not null)
            {
                services.AddTransient(specificationInterface, specType);
            }
        }
    }

    private static bool IsConcreteSpecification(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            return false;
        }

        // Check if the type implements ISpecification<T> or derives from Specification<T>
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(ISpecification<>));
    }

    private static bool HasConstructorDependencies(Type type)
    {
        // Only register specifications that have constructor dependencies
        // (parameterless specs can be newed directly)
        return type.GetConstructors()
            .Any(c => c.GetParameters().Length > 0 &&
                      c.GetParameters().Any(p => !p.HasDefaultValue));
    }
}

internal static partial class SpecificationServiceCollectionExtensionsLog
{
    [LoggerMessage(LogLevel.Information, "Specification diagnostics activated")]
    public static partial void LogDiagnosticsActivated(this ILogger logger);
}
