using System.Reflection;
using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture.Domain;

namespace FamilyHub.Tests.Architecture.Helpers;

/// <summary>
/// Base class providing common infrastructure for architecture tests.
/// Centralizes assembly references and helper methods.
/// </summary>
public abstract class ArchitectureTestBase
{
    #region Production Assemblies

    /// <summary>
    /// Auth module assembly (from CompleteZitadelLoginCommand).
    /// </summary>
    protected static readonly Assembly AuthModuleAssembly =
        typeof(CompleteZitadelLoginCommand).Assembly;

    /// <summary>
    /// Family module assembly (from Family aggregate).
    /// </summary>
    protected static readonly Assembly FamilyModuleAssembly =
        typeof(Family).Assembly;

    /// <summary>
    /// SharedKernel assembly (from AggregateRoot base class).
    /// </summary>
    protected static readonly Assembly SharedKernelAssembly =
        typeof(AggregateRoot<>).Assembly;

    /// <summary>
    /// All production module assemblies for iteration.
    /// </summary>
    protected static readonly Assembly[] AllModuleAssemblies =
    [
        AuthModuleAssembly,
        FamilyModuleAssembly
    ];

    #endregion

    #region Violation Fixtures Assembly

    /// <summary>
    /// Assembly containing intentional architecture violations for negative testing.
    /// </summary>
    protected static readonly Assembly ViolationFixturesAssembly =
        typeof(DomainDependingOnApplication).Assembly;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Formats failing type names for assertion messages.
    /// </summary>
    protected static string FormatFailingTypes(IEnumerable<string>? failingTypes)
    {
        if (failingTypes == null)
        {
            return "None";
        }

        var failingTypeList = failingTypes.ToList();
        return failingTypeList.Count == 0 ? "None" : string.Join(", ", failingTypeList);
    }

    /// <summary>
    /// Asserts that a NetArchTest rule succeeded.
    /// </summary>
    protected static void AssertRuleSucceeds(TestResult result, string ruleName)
    {
        result.IsSuccessful.Should().BeTrue(
            because: $"Rule '{ruleName}' should pass. " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// Asserts that a NetArchTest rule failed (for negative testing).
    /// Used to verify that architecture rules actually detect violations.
    /// </summary>
    protected static void AssertRuleFails(TestResult result, string ruleName)
    {
        result.IsSuccessful.Should().BeFalse(
            because: $"Negative test for rule '{ruleName}' should detect violations. " +
                     "If this passes (IsSuccessful=true), the rule may not be working correctly.");
    }

    /// <summary>
    /// Asserts that a specific type is among the failing types.
    /// </summary>
    protected static void AssertTypeInFailures(TestResult result, string expectedTypeName)
    {
        result.FailingTypeNames.Should().Contain(
            t => t.Contains(expectedTypeName),
            because: $"Expected type '{expectedTypeName}' should be detected as a violation.");
    }

    /// <summary>
    /// Guards against empty type collections in tests.
    /// Ensures we're actually testing something.
    /// </summary>
    protected static void GuardAgainstEmptyTypes(
        IEnumerable<Type> types,
        string description)
    {
        types.Should().NotBeEmpty(
            because: $"{description} should contain types to test. " +
                     "An empty collection could lead to false positives.");
    }

    #endregion
}
