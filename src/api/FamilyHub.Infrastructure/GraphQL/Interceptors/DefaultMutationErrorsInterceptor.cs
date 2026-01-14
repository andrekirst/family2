using System.Reflection;
using FamilyHub.SharedKernel.Presentation.GraphQL.Errors;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

namespace FamilyHub.Infrastructure.GraphQL.Interceptors;

/// <summary>
/// Custom attribute that automatically applies all error types implementing
/// <see cref="IDefaultMutationError"/> to a GraphQL mutation field.
/// </summary>
/// <remarks>
/// <para>
/// This attribute follows the Open-Closed Principle: to add a new default error type,
/// simply create a class that inherits from <see cref="BaseError"/> and implements
/// <see cref="IDefaultMutationError"/>. No modification to this attribute is required.
/// </para>
/// <para>
/// Usage:
/// <code>
/// [DefaultMutationErrors]
/// [UseMutationConvention]
/// public async Task&lt;Result&gt; MyMutation(MyInput input) { ... }
/// </code>
/// </para>
/// <para>
/// Mutation-specific errors can still be declared using <c>[Error(typeof(...))]</c>
/// attributes; they will be merged with the default errors.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DefaultMutationErrorsAttribute : ObjectFieldDescriptorAttribute
{
    private static readonly Lazy<Type[]> DefaultErrorTypes = new(DiscoverDefaultErrorTypes);
    private static readonly Lazy<MethodInfo?> ErrorMethod = new(DiscoverErrorMethod);

    /// <summary>
    /// Discovers all types implementing <see cref="IDefaultMutationError"/> from the SharedKernel assembly.
    /// </summary>
    private static Type[] DiscoverDefaultErrorTypes()
    {
        var sharedKernelAssembly = Assembly.GetAssembly(typeof(IDefaultMutationError))
            ?? throw new InvalidOperationException(
                "Could not find assembly containing IDefaultMutationError.");

        return sharedKernelAssembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(IDefaultMutationError).IsAssignableFrom(t))
            .ToArray();
    }

    /// <summary>
    /// Discovers the Error&lt;T&gt; extension method from HotChocolate.Types.Mutations assembly.
    /// In Hot Chocolate 14+, mutation conventions (including Error&lt;T&gt;) are in a separate assembly.
    /// </summary>
    private static MethodInfo? DiscoverErrorMethod()
    {
        // Search all loaded assemblies for the Error<T> extension method
        // In Hot Chocolate 14+, it's in HotChocolate.Types.Mutations
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.StartsWith("HotChocolate", StringComparison.OrdinalIgnoreCase) == true);

        foreach (var assembly in assemblies)
        {
            try
            {
                // Search for extension classes that have an Error<T> method for IObjectFieldDescriptor
                var extensionTypes = assembly
                    .GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: true, IsSealed: true }); // Static classes

                foreach (var extensionType in extensionTypes)
                {
                    var method = extensionType
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(m => m is { Name: "Error", IsGenericMethod: true }
                                             && m.GetParameters().Length == 1
                                             && m.GetParameters()[0].ParameterType == typeof(IObjectFieldDescriptor));

                    if (method is not null)
                    {
                        return method;
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Some assemblies may have loading issues, skip them
            }
        }

        return null;
    }

    /// <inheritdoc />
    protected override void OnConfigure(
        IDescriptorContext context,
        IObjectFieldDescriptor descriptor,
        MemberInfo member)
    {
        var errorMethod = ErrorMethod.Value
            ?? throw new InvalidOperationException(
                "Could not find Error<T> extension method for IObjectFieldDescriptor. " +
                "Ensure HotChocolate.Types package is referenced and mutation conventions are enabled.");

        foreach (var errorType in DefaultErrorTypes.Value)
        {
            var genericErrorMethod = errorMethod.MakeGenericMethod(errorType);
            genericErrorMethod.Invoke(null, [descriptor]);
        }
    }
}
