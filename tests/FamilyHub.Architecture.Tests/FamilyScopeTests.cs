using System.Reflection;
using FamilyHub.Common.Application;
using FluentAssertions;

namespace FamilyHub.Architecture.Tests;

/// <summary>
/// Ensures all commands and queries declare their user/family intent
/// via IRequireFamily, IRequireUser, or IAnonymousOperation.
/// </summary>
public class FamilyScopeTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void All_commands_should_declare_family_scope_intent()
    {
        var commandTypes = GetMessageTypes(typeof(ICommand<>));
        AssertFamilyScopeDeclaration(commandTypes);
    }

    [Fact]
    public void All_queries_should_declare_family_scope_intent()
    {
        var queryTypes = GetMessageTypes(typeof(IQuery<>));
        AssertFamilyScopeDeclaration(queryTypes);
    }

    private static List<Type> GetMessageTypes(Type openGenericInterface)
    {
        return ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface))
            .ToList();
    }

    private static void AssertFamilyScopeDeclaration(List<Type> types)
    {
        var violations = new List<string>();

        foreach (var type in types)
        {
            var isRequireFamily = typeof(IRequireFamily).IsAssignableFrom(type);
            var isRequireUser = typeof(IRequireUser).IsAssignableFrom(type);
            var isAnonymousOperation = typeof(IAnonymousOperation).IsAssignableFrom(type);

            if (!isRequireFamily && !isRequireUser && !isAnonymousOperation)
            {
                violations.Add($"{type.Name} must implement IRequireFamily, IRequireUser, or IAnonymousOperation");
            }
        }

        violations.Should().BeEmpty(
            "all commands/queries must declare their user/family intent");
    }
}
