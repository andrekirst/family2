using System.Reflection;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Infrastructure.GraphQL.Extensions;

/// <summary>
/// Extension methods for automatic GraphQL type extension registration.
/// </summary>
public static class GraphQlTypeExtensions
{
    /// <summary>
    /// Automatically discovers and registers all GraphQL type extensions from the specified assemblies.
    /// Type extensions are identified by the presence of the [ExtendObjectType] attribute.
    /// </summary>
    /// <param name="builder">The GraphQL request executor builder.</param>
    /// <param name="assemblies">The assemblies to scan for type extensions.</param>
    /// <param name="loggerFactory">Optional logger factory for diagnostics and debugging.</param>
    /// <returns>The builder for method chaining.</returns>
    /// <remarks>
    /// This method uses reflection to discover types with the [ExtendObjectType] attribute
    /// and dynamically registers them with Hot Chocolate's AddTypeExtension&lt;T&gt;() method.
    ///
    /// Typical usage:
    /// <code>
    /// builder.AddTypeExtensionsFromAssemblies(
    ///     new[] { typeof(MyModule).Assembly },
    ///     loggerFactory);
    /// </code>
    /// </remarks>
    public static IRequestExecutorBuilder AddTypeExtensionsFromAssemblies(
        this IRequestExecutorBuilder builder,
        IEnumerable<Assembly> assemblies,
        ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(assemblies);

        var logger = loggerFactory?.CreateLogger("GraphQLTypeExtensions");
        var typeExtensions = new List<Type>();
        var assemblyList = assemblies.ToList();

        // Scan assemblies for types with [ExtendObjectType] attribute
        foreach (var assembly in assemblyList)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(type => type is { IsClass: true, IsAbstract: false } &&
                                   type.GetCustomAttribute<ExtendObjectTypeAttribute>() != null)
                    .ToList();

                if (types.Count == 0)
                {
                    continue;
                }

                typeExtensions.AddRange(types);
                logger?.LogFoundExtensions(types.Count, assembly.GetName().Name);
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger?.LogFailedToLoadTypes(assembly.GetName().Name, ex);
            }
        }

        // If no type extensions found, log warning and return
        if (typeExtensions.Count == 0)
        {
            logger?.LogNoExtensionsFound(string.Join(", ", assemblyList.Select(a => a.GetName().Name)));
            return builder;
        }

        // Log summary of discovered type extensions
        logger?.LogRegisteringExtensions(
            typeExtensions.Count,
            assemblyList.Count,
            assemblyList.Count == 1 ? "y" : "ies",
            string.Join(", ", assemblyList.Select(a => a.GetName().Name)));

        // Register each discovered type extension
        foreach (var type in typeExtensions)
        {
            try
            {
                logger?.LogRegisteringType(type.Name);

                // Find the AddTypeExtension<T>() extension method in Hot Chocolate assemblies
                var method = typeof(IRequestExecutorBuilder).Assembly
                    .GetTypes()
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(m => m is { Name: "AddTypeExtension", IsGenericMethod: true } &&
                                         m.GetParameters().Length == 1 &&
                                         m.GetParameters()[0].ParameterType == typeof(IRequestExecutorBuilder));

                if (method == null)
                {
                    logger?.LogAddTypeExtensionMethodNotFound();
                    continue;
                }

                // Make the generic method with the discovered type
                var genericMethod = method.MakeGenericMethod(type);

                // Invoke the method: builder.AddTypeExtension<T>()
                genericMethod.Invoke(null, [builder]);

                logger?.LogRegistrationSuccess(type.FullName);
            }
            catch (Exception ex)
            {
                logger?.LogRegistrationFailed(type.FullName, ex);
                throw;
            }
        }

        return builder;
    }
}

internal static partial class GraphQlTypeExtensionsLog
{
    [LoggerMessage(LogLevel.Debug, "Found {Count} GraphQL type extension(s) in assembly {AssemblyName}")]
    public static partial void LogFoundExtensions(this ILogger logger, int count, string? assemblyName);

    [LoggerMessage(LogLevel.Warning, "Failed to load types from assembly {AssemblyName}. Skipping...")]
    public static partial void LogFailedToLoadTypes(this ILogger logger, string? assemblyName, Exception exception);

    [LoggerMessage(LogLevel.Warning, "No GraphQL type extensions found in assemblies: {Assemblies}")]
    public static partial void LogNoExtensionsFound(this ILogger logger, string assemblies);

    [LoggerMessage(LogLevel.Information, "Registering {Count} GraphQL type extension(s) from {AssemblyCount} assembl{Suffix}: {Assemblies}")]
    public static partial void LogRegisteringExtensions(this ILogger logger, int count, int assemblyCount, string suffix, string assemblies);

    [LoggerMessage(LogLevel.Debug, "Registering GraphQL type extension: {TypeName}")]
    public static partial void LogRegisteringType(this ILogger logger, string typeName);

    [LoggerMessage(LogLevel.Error, "Could not find AddTypeExtension method in Hot Chocolate assembly. API may have changed.")]
    public static partial void LogAddTypeExtensionMethodNotFound(this ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Successfully registered: {TypeName}")]
    public static partial void LogRegistrationSuccess(this ILogger logger, string? typeName);

    [LoggerMessage(LogLevel.Error, "Failed to register GraphQL type extension: {TypeName}")]
    public static partial void LogRegistrationFailed(this ILogger logger, string? typeName, Exception exception);
}
