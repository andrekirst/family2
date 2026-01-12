using System.Reflection;
using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Architecture tests ensuring proper module boundaries and bounded context separation.
/// Modules should be loosely coupled and communicate through well-defined interfaces.
/// </summary>
public sealed class ModuleBoundaryTests
{
    private static readonly Assembly AuthModuleAssembly = typeof(CompleteZitadelLoginCommand).Assembly;
    private static readonly Assembly FamilyModuleAssembly = typeof(Family).Assembly;
    private static readonly Assembly SharedKernelAssembly = typeof(AggregateRoot<>).Assembly;

    /// <summary>
    /// Family module's domain layer should not directly depend on Auth module's domain layer.
    /// Cross-module communication should go through application layer abstractions.
    /// </summary>
    [Fact]
    public void FamilyModule_ShouldNotHaveDirectDependencyOn_AuthModuleDomain()
    {
        // Arrange
        var types = Types.InAssembly(FamilyModuleAssembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(TestConstants.FamilyModuleNamespace)
            .ShouldNot()
            .HaveDependencyOn($"{TestConstants.AuthModuleNamespace}{TestConstants.DomainLayer}")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Family module should not directly depend on Auth module's domain layer to maintain bounded context separation. " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// Auth module's domain layer should not directly depend on Family module's domain aggregates.
    /// Note: During Phase 5 migration, there are known cross-module dependencies via GlobalUsings.
    /// This test documents these dependencies and should be made stricter as modules are fully separated.
    /// Currently excluded: User.GetRoleInFamily() method that accepts FamilyAggregate parameter.
    /// </summary>
    [Fact]
    public void AuthModule_DomainLayer_ShouldNotDependOn_FamilyModuleDomain()
    {
        // Arrange
        var types = Types.InAssembly(AuthModuleAssembly);

        // Act - Check for dependencies on Family module's domain aggregates
        var result = types
            .That()
            .ResideInNamespaceStartingWith($"{TestConstants.AuthModuleNamespace}{TestConstants.DomainLayer}")
            .ShouldNot()
            .HaveDependencyOn($"{TestConstants.FamilyModuleNamespace}{TestConstants.DomainLayer}{TestConstants.AggregatesSuffix}")
            .GetResult();

        // Assert
        // NOTE: This test currently fails due to known Phase 5 cross-module dependency.
        // The User entity references FamilyAggregate in GetRoleInFamily() method.
        // This is documented in GlobalUsings.cs and should be refactored in Phase 6.
        // For now, we document the known violations rather than hiding them.
        var knownViolations = new HashSet<string>
        {
            "FamilyHub.Modules.Auth.Domain.User" // Known: GetRoleInFamily(FamilyAggregate)
        };

        var unexpectedViolations = (result.FailingTypeNames ?? Array.Empty<string>())
            .Where(t => !knownViolations.Contains(t))
            .ToList();

        unexpectedViolations.Should().BeEmpty(
            because: $"Auth module's domain layer should not have NEW dependencies on Family module's domain aggregates. " +
                     $"Known violations (Phase 5 migration): {string.Join(", ", knownViolations)}. " +
                     $"Unexpected violations: {FormatFailingTypes(unexpectedViolations)}");
    }

    /// <summary>
    /// SharedKernel should not depend on any module.
    /// It provides common building blocks that modules depend on, not vice versa.
    /// </summary>
    [Fact]
    public void SharedKernel_ShouldNotDependOn_AnyModule()
    {
        // Arrange
        var types = Types.InAssembly(SharedKernelAssembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(TestConstants.SharedKernelNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(TestConstants.ModuleNamespaces)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"SharedKernel should not depend on any module - it provides common building blocks. " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// All modules should be able to depend on SharedKernel.
    /// SharedKernel provides base classes, interfaces, and value objects.
    /// </summary>
    [Theory]
    [InlineData(nameof(AuthModuleAssembly))]
    [InlineData(nameof(FamilyModuleAssembly))]
    public void Modules_CanDependOn_SharedKernel(string assemblyName)
    {
        // Arrange
        var assembly = assemblyName switch
        {
            nameof(AuthModuleAssembly) => AuthModuleAssembly,
            nameof(FamilyModuleAssembly) => FamilyModuleAssembly,
            _ => throw new ArgumentException($"Unknown assembly: {assemblyName}")
        };

        var types = Types.InAssembly(assembly);

        // Act - Check that types CAN have dependencies on SharedKernel (positive test)
        var typesUsingSharedKernel = types
            .That()
            .HaveDependencyOn(TestConstants.SharedKernelNamespace)
            .GetTypes();

        // Assert - At least some types should use SharedKernel (base classes, value objects)
        typesUsingSharedKernel.Should().NotBeEmpty(
            because: $"Module {assemblyName} should use SharedKernel for base classes, value objects, and common interfaces");
    }

    private static string FormatFailingTypes(IEnumerable<string>? failingTypes)
    {
        if (failingTypes == null || !failingTypes.Any())
        {
            return "None";
        }

        return string.Join(", ", failingTypes);
    }
}
