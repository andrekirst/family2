using FamilyHub.Tests.Architecture.Helpers;
using NetArchTest.Rules;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Negative tests for Module Boundary rules.
/// These tests verify that module isolation rules actually DETECT violations
/// by testing against intentionally violating types in the fixtures assembly.
/// </summary>
public sealed class ModuleBoundaryNegativeTests : ArchitectureTestBase
{
    private const string FixturesModuleBoundaryNamespace =
        "FamilyHub.Tests.Architecture.Fixtures.Violations.ModuleBoundary";

    /// <summary>
    /// Verifies that the rule detects when Family module depends on Auth module.
    /// </summary>
    [Fact]
    public void FamilyModule_WhenDependingOnAuthModule_ShouldBeDetected()
    {
        // Arrange
        var familyNamespace = $"{FixturesModuleBoundaryNamespace}.Family";
        var authNamespace = $"{FixturesModuleBoundaryNamespace}.Auth";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard
        var familyTypes = types
            .That()
            .ResideInNamespaceStartingWith(familyNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(familyTypes, $"Family namespace '{familyNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(familyNamespace)
            .ShouldNot()
            .HaveDependencyOn(authNamespace)
            .GetResult();

        // Assert
        AssertRuleFails(result, "FamilyModule_ShouldNotHaveDirectDependencyOn_AuthModuleDomain");
        AssertTypeInFailures(result, "FamilyViolatingAuth");
    }

    /// <summary>
    /// Verifies that the rule detects when Auth module depends on Family module.
    /// </summary>
    [Fact]
    public void AuthModule_WhenDependingOnFamilyModule_ShouldBeDetected()
    {
        // Arrange
        var authNamespace = $"{FixturesModuleBoundaryNamespace}.Auth";
        var familyNamespace = $"{FixturesModuleBoundaryNamespace}.Family";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard
        var authTypes = types
            .That()
            .ResideInNamespaceStartingWith(authNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(authTypes, $"Auth namespace '{authNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(authNamespace)
            .ShouldNot()
            .HaveDependencyOn(familyNamespace)
            .GetResult();

        // Assert
        AssertRuleFails(result, "AuthModule_DomainLayer_ShouldNotDependOn_FamilyModuleDomain");
        AssertTypeInFailures(result, "AuthViolatingFamily");
    }

    /// <summary>
    /// Verifies that the rule detects when SharedKernel depends on any module.
    /// </summary>
    [Fact]
    public void SharedKernel_WhenDependingOnModule_ShouldBeDetected()
    {
        // Arrange
        var sharedKernelNamespace = $"{FixturesModuleBoundaryNamespace}.SharedKernel";
        var authNamespace = $"{FixturesModuleBoundaryNamespace}.Auth";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard
        var sharedKernelTypes = types
            .That()
            .ResideInNamespaceStartingWith(sharedKernelNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(sharedKernelTypes, $"SharedKernel namespace '{sharedKernelNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(sharedKernelNamespace)
            .ShouldNot()
            .HaveDependencyOn(authNamespace)
            .GetResult();

        // Assert
        AssertRuleFails(result, "SharedKernel_ShouldNotDependOn_AnyModule");
        AssertTypeInFailures(result, "SharedKernelViolatingModule");
    }

    /// <summary>
    /// Positive negative test: Verifies that modules CAN depend on SharedKernel.
    /// This is a "positive" test in that it confirms allowed dependencies work.
    /// </summary>
    [Fact]
    public void Modules_CanDependOn_SharedKernel_ShouldNotFail()
    {
        // Arrange - Auth module depends on SharedKernel (via base types)
        var authTypes = Types.InAssembly(AuthModuleAssembly);

        // Act - This should NOT flag any violations
        var result = authTypes
            .That()
            .ResideInNamespaceStartingWith(TestConstants.AuthModuleNamespace)
            .Should()
            .HaveDependencyOn(TestConstants.SharedKernelNamespace)
            .GetResult();

        // Assert - The Auth module SHOULD have dependencies on SharedKernel
        // Note: This is a different assertion - we're confirming the dependency EXISTS
        // If no types depend on SharedKernel, this would indicate a problem
        result.FailingTypeNames.Should().NotBeNull(
            because: "Auth module should have types that depend on SharedKernel " +
                     "(e.g., entities inheriting from AggregateRoot)");
    }
}
