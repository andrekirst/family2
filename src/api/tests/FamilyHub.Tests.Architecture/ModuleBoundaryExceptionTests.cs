using FamilyHub.Tests.Architecture.Helpers;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Tests for the Exception Registry and documented architecture exceptions.
/// These tests ensure:
/// 1. Documented exceptions are still valid (type still exists and still violates)
/// 2. No new undocumented violations are introduced
/// 3. Exception metadata is complete and valid
/// </summary>
public sealed class ModuleBoundaryExceptionTests : ArchitectureTestBase
{
    /// <summary>
    /// Verifies that all documented exceptions in the registry are still actual violations.
    /// If an exception is no longer violating, it should be removed from the registry.
    /// This prevents exception rot - keeping exceptions that are no longer needed.
    /// </summary>
    [Fact]
    public void DocumentedExceptions_ShouldStillViolate_OrBeRemoved()
    {
        // Arrange
        var types = Types.InAssembly(AuthModuleAssembly);

        // Act - Check for actual violations
        var result = types
            .That()
            .ResideInNamespaceStartingWith(TestConstants.AuthModuleNamespace)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.FamilyModuleNamespace)
            .GetResult();

        var actualViolations = result.FailingTypeNames?.ToHashSet() ?? [];

        // Assert - Each documented exception should still be violating
        foreach (var (typeName, reason) in ExceptionRegistry.ModuleBoundaryExceptions)
        {
            if (!actualViolations.Contains(typeName))
            {
                Assert.Fail(
                    $"Documented exception '{typeName}' no longer violates the module boundary rule. " +
                    $"Original reason: {reason.Reason} ({reason.Phase}). " +
                    $"Ticket: {reason.Ticket}. " +
                    $"ACTION REQUIRED: Remove this entry from ExceptionRegistry.ModuleBoundaryExceptions.");
            }
        }
    }

    /// <summary>
    /// Verifies that no new violations are introduced without being documented.
    /// Any new violation must be added to the ExceptionRegistry with proper metadata.
    /// </summary>
    [Fact]
    public void NewViolations_ShouldBeDocumented_OrFail()
    {
        // Arrange
        var types = Types.InAssembly(AuthModuleAssembly);

        // Act - Check for actual violations
        var result = types
            .That()
            .ResideInNamespaceStartingWith(TestConstants.AuthModuleNamespace)
            .ShouldNot()
            .HaveDependencyOn(TestConstants.FamilyModuleNamespace)
            .GetResult();

        // Filter out documented exceptions
        var unexpectedViolations = ExceptionRegistry.FilterKnownViolations(
            result.FailingTypeNames,
            ExceptionRegistry.ModuleBoundaryExceptions);

        // Assert
        unexpectedViolations.Should().BeEmpty(
            because: ExceptionRegistry.BuildExceptionMessage(
                "AuthModule_ShouldNotDependOn_FamilyModule",
                unexpectedViolations,
                ExceptionRegistry.ModuleBoundaryExceptions));
    }

    /// <summary>
    /// Verifies that all exception entries have complete metadata.
    /// Each exception must have: Phase, Reason, Ticket, and PlannedRemoval.
    /// </summary>
    [Fact]
    public void ExceptionRegistry_ShouldHaveCompleteMetadata()
    {
        // Arrange & Act
        var incompleteExceptions = new List<string>();

        foreach (var (category, exceptions) in ExceptionRegistry.GetAllExceptionRegistries())
        {
            foreach (var (typeName, reason) in exceptions)
            {
                var issues = new List<string>();

                if (string.IsNullOrWhiteSpace(reason.Phase))
                {
                    issues.Add("missing Phase");
                }

                if (string.IsNullOrWhiteSpace(reason.Reason))
                {
                    issues.Add("missing Reason");
                }

                if (string.IsNullOrWhiteSpace(reason.Ticket))
                {
                    issues.Add("missing Ticket");
                }

                if (string.IsNullOrWhiteSpace(reason.PlannedRemoval))
                {
                    issues.Add("missing PlannedRemoval");
                }

                if (issues.Count > 0)
                {
                    incompleteExceptions.Add($"{category}.{typeName}: {string.Join(", ", issues)}");
                }
            }
        }

        // Assert
        incompleteExceptions.Should().BeEmpty(
            because: "All exception entries must have complete metadata for tracking. " +
                     $"Incomplete entries: [{string.Join("; ", incompleteExceptions)}]");
    }

    /// <summary>
    /// Verifies that Family module doesn't have undocumented dependencies on Auth module.
    /// </summary>
    [Fact]
    public void FamilyModule_ShouldNotHaveUndocumentedDependenciesOn_AuthModule()
    {
        // Arrange
        var types = Types.InAssembly(FamilyModuleAssembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(TestConstants.FamilyModuleNamespace)
            .ShouldNot()
            .HaveDependencyOn($"{TestConstants.AuthModuleNamespace}.Domain")
            .GetResult();

        // Assert - Family module should have NO dependencies on Auth module domain
        result.IsSuccessful.Should().BeTrue(
            because: $"Family module should not depend on Auth module's domain. " +
                     $"Modules should communicate through SharedKernel abstractions or domain events. " +
                     $"Violations: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// Verifies that SharedKernel has no dependencies on any module.
    /// SharedKernel is the foundation layer with zero outward dependencies.
    /// </summary>
    [Fact]
    public void SharedKernel_ShouldHaveNoUndocumentedModuleDependencies()
    {
        // Arrange
        var types = Types.InAssembly(SharedKernelAssembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(TestConstants.SharedKernelNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(
                TestConstants.AuthModuleNamespace,
                TestConstants.FamilyModuleNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"SharedKernel must not depend on any module. " +
                     $"It provides foundation types used BY modules, not types that USE modules. " +
                     $"Violations: {FormatFailingTypes(result.FailingTypeNames)}");
    }
}
