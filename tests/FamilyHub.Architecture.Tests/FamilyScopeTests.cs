using System.Reflection;
using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FluentAssertions;

namespace FamilyHub.Architecture.Tests;

/// <summary>
/// Ensures all commands and queries declare their family membership intent
/// via IFamilyScoped, IIgnoreFamilyMembership, or are in the known fallback set.
/// </summary>
public class FamilyScopeTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    /// <summary>
    /// Commands/queries that intentionally use the fallback path
    /// (user must belong to ANY family, but no FamilyId is available on the message).
    /// </summary>
    private static readonly HashSet<string> AllowedFallbackTypes =
    [
        // GoogleIntegration — implicitly family-scoped via authenticated user
        "LinkGoogleAccountCommand",
        "RefreshGoogleTokenCommand",
        "UnlinkGoogleAccountCommand",
        "GetGoogleAuthUrlQuery",
        // Dashboard — scoped via user's dashboard, not FamilyId
        "AddWidgetCommand",
        "RemoveWidgetCommand",
        "ResetDashboardCommand",
        "UpdateWidgetConfigCommand",
        // Calendar — scoped via event ID, not FamilyId
        "CancelCalendarEventCommand",
        "UpdateCalendarEventCommand",
        // Dashboard
        "SaveDashboardLayoutCommand",
    ];

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
            var isFamilyScoped = typeof(IFamilyScoped).IsAssignableFrom(type);
            var isIgnored = typeof(IIgnoreFamilyMembership).IsAssignableFrom(type);
            var isAllowedFallback = AllowedFallbackTypes.Contains(type.Name);

            if (!isFamilyScoped && !isIgnored && !isAllowedFallback)
            {
                violations.Add($"{type.Name} must implement IFamilyScoped, IIgnoreFamilyMembership, or be in AllowedFallbackTypes");
            }
        }

        violations.Should().BeEmpty(
            "all commands/queries must declare their family membership intent");
    }
}
