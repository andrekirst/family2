using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

                if (types.Count == 0) continue;
                
                typeExtensions.AddRange(types);
                logger?.LogDebug(
                    "Found {Count} GraphQL type extension(s) in assembly {AssemblyName}",
                    types.Count,
                    assembly.GetName().Name);
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger?.LogWarning(
                    ex,
                    "Failed to load types from assembly {AssemblyName}. Skipping...",
                    assembly.GetName().Name);
            }
        }

        // If no type extensions found, log warning and return
        if (typeExtensions.Count == 0)
        {
            logger?.LogWarning(
                "No GraphQL type extensions found in assemblies: {Assemblies}",
                string.Join(", ", assemblyList.Select(a => a.GetName().Name)));
            return builder;
        }

        // Log summary of discovered type extensions
        logger?.LogInformation(
            "Registering {Count} GraphQL type extension(s) from {AssemblyCount} assembl{ies}: {Assemblies}",
            typeExtensions.Count,
            assemblyList.Count,
            assemblyList.Count == 1 ? "y" : "ies",
            string.Join(", ", assemblyList.Select(a => a.GetName().Name)));

        // Register each discovered type extension
        foreach (var type in typeExtensions)
        {
            try
            {
                logger?.LogDebug("Registering GraphQL type extension: {TypeName}", type.Name);

                // Find the AddTypeExtension<T>() extension method in Hot Chocolate assemblies
                var method = typeof(IRequestExecutorBuilder).Assembly
                    .GetTypes()
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(m => m is { Name: "AddTypeExtension", IsGenericMethod: true } &&
                                         m.GetParameters().Length == 1 &&
                                         m.GetParameters()[0].ParameterType == typeof(IRequestExecutorBuilder));

                if (method == null)
                {
                    logger?.LogError(
                        "Could not find AddTypeExtension method in Hot Chocolate assembly. API may have changed.");
                    continue;
                }

                // Make the generic method with the discovered type
                var genericMethod = method.MakeGenericMethod(type);

                // Invoke the method: builder.AddTypeExtension<T>()
                genericMethod.Invoke(null, [builder]);

                logger?.LogDebug("Successfully registered: {TypeName}", type.FullName);
            }
            catch (Exception ex)
            {
                logger?.LogError(
                    ex,
                    "Failed to register GraphQL type extension: {TypeName}",
                    type.FullName);
                throw;
            }
        }

        return builder;
    }
}
