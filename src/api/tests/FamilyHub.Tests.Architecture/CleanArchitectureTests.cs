using System.Reflection;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Family.Domain.Aggregates;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Architecture tests ensuring clean architecture layer dependencies.
/// Dependencies should flow inward: Presentation -> Application -> Domain.
/// Domain layer should have no dependencies on outer layers.
/// </summary>
public sealed class CleanArchitectureTests
{
    private static readonly Assembly AuthModuleAssembly = typeof(CreateFamilyCommand).Assembly;
    private static readonly Assembly FamilyModuleAssembly = typeof(Family).Assembly;

    /// <summary>
    /// Layer test data for parameterized tests.
    /// </summary>
    public static TheoryData<string, Assembly> ModuleAssemblies => new()
    {
        { TestConstants.AuthModuleNamespace, AuthModuleAssembly },
        { TestConstants.FamilyModuleNamespace, FamilyModuleAssembly }
    };

    /// <summary>
    /// Domain layer should not depend on Application layer.
    /// Domain contains business logic and entities that are independent of use cases.
    /// </summary>
    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void DomainLayer_ShouldNotDependOn_ApplicationLayer(string moduleNamespace, Assembly assembly)
    {
        // Arrange
        var domainNamespace = $"{moduleNamespace}{TestConstants.DomainLayer}";
        var applicationNamespace = $"{moduleNamespace}{TestConstants.ApplicationLayer}";
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .ShouldNot()
            .HaveDependencyOn(applicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Domain layer ({domainNamespace}) should not depend on Application layer ({applicationNamespace}). " +
                     $"Domain contains core business logic that must remain independent of use cases. " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// Domain layer should not depend on Persistence layer.
    /// Domain defines repository interfaces, but implementations live in Persistence.
    /// </summary>
    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void DomainLayer_ShouldNotDependOn_PersistenceLayer(string moduleNamespace, Assembly assembly)
    {
        // Arrange
        var domainNamespace = $"{moduleNamespace}{TestConstants.DomainLayer}";
        var persistenceNamespace = $"{moduleNamespace}{TestConstants.PersistenceLayer}";
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .ShouldNot()
            .HaveDependencyOn(persistenceNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Domain layer ({domainNamespace}) should not depend on Persistence layer ({persistenceNamespace}). " +
                     $"Domain defines repository interfaces; Persistence provides implementations. " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// Domain layer should not depend on Presentation layer.
    /// Domain is the innermost layer and should have no knowledge of how it's presented.
    /// </summary>
    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void DomainLayer_ShouldNotDependOn_PresentationLayer(string moduleNamespace, Assembly assembly)
    {
        // Arrange
        var domainNamespace = $"{moduleNamespace}{TestConstants.DomainLayer}";
        var presentationNamespace = $"{moduleNamespace}{TestConstants.PresentationLayer}";
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .ShouldNot()
            .HaveDependencyOn(presentationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Domain layer ({domainNamespace}) should not depend on Presentation layer ({presentationNamespace}). " +
                     $"Domain is the innermost layer with no knowledge of presentation concerns. " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// Application layer should not depend on Presentation layer.
    /// Application contains use cases that are independent of the delivery mechanism.
    /// </summary>
    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void ApplicationLayer_ShouldNotDependOn_PresentationLayer(string moduleNamespace, Assembly assembly)
    {
        // Arrange
        var applicationNamespace = $"{moduleNamespace}{TestConstants.ApplicationLayer}";
        var presentationNamespace = $"{moduleNamespace}{TestConstants.PresentationLayer}";
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(applicationNamespace)
            .ShouldNot()
            .HaveDependencyOn(presentationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Application layer ({applicationNamespace}) should not depend on Presentation layer ({presentationNamespace}). " +
                     $"Use cases should be independent of how they are invoked (GraphQL, REST, etc.). " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
    }

    /// <summary>
    /// Application layer should not depend on Persistence implementations.
    /// Application uses repository interfaces from Domain, not concrete implementations.
    /// </summary>
    [Theory]
    [MemberData(nameof(ModuleAssemblies))]
    public void ApplicationLayer_ShouldNotDependOn_PersistenceImplementations(string moduleNamespace, Assembly assembly)
    {
        // Arrange
        var applicationNamespace = $"{moduleNamespace}{TestConstants.ApplicationLayer}";
        var persistenceRepositoriesNamespace = $"{moduleNamespace}{TestConstants.PersistenceLayer}.Repositories";
        var types = Types.InAssembly(assembly);

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(applicationNamespace)
            .ShouldNot()
            .HaveDependencyOn(persistenceRepositoriesNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            because: $"Application layer ({applicationNamespace}) should depend on repository interfaces from Domain, " +
                     $"not concrete implementations from Persistence ({persistenceRepositoriesNamespace}). " +
                     $"Failing types: {FormatFailingTypes(result.FailingTypeNames)}");
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
