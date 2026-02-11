using System.Reflection;
using HotChocolate.Execution.Configuration;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

public static class RequestExecutorBuilderExtensions
{
    /// <summary>
    /// Discovers and registers all classes annotated with [ExtendObjectType]
    /// from the given assembly as Hot Chocolate type extensions.
    /// </summary>
    public static IRequestExecutorBuilder AddTypeExtensionsFromAssembly(
        this IRequestExecutorBuilder builder, Assembly assembly)
    {
        var extensionTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                && Attribute.IsDefined(t, typeof(ExtendObjectTypeAttribute), inherit: true));

        foreach (var type in extensionTypes)
        {
            builder.AddTypeExtension(type);
        }

        return builder;
    }
}
