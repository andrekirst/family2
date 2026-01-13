using FamilyHub.Tests.Architecture.Helpers;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Negative tests for Clean Architecture rules.
/// These tests verify that architecture rules actually DETECT violations
/// by testing against intentionally violating types in the fixtures assembly.
/// </summary>
public sealed class CleanArchitectureNegativeTests : ArchitectureTestBase
{
    private const string FixturesCleanArchNamespace =
        "FamilyHub.Tests.Architecture.Fixtures.Violations.CleanArchitecture";

    /// <summary>
    /// Verifies that the rule detects when Domain depends on Application.
    /// </summary>
    [Fact]
    public void DomainLayer_WhenDependingOnApplication_ShouldBeDetected()
    {
        // Arrange
        var domainNamespace = $"{FixturesCleanArchNamespace}.Domain";
        var applicationNamespace = $"{FixturesCleanArchNamespace}.Application";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard - ensure we have types to test
        var domainTypes = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(domainTypes, $"Domain namespace '{domainNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .ShouldNot()
            .HaveDependencyOn(applicationNamespace)
            .GetResult();

        // Assert - Rule should FAIL (detecting the violation)
        AssertRuleFails(result, "DomainLayer_ShouldNotDependOn_ApplicationLayer");
        AssertTypeInFailures(result, "DomainDependingOnApplication");
    }

    /// <summary>
    /// Verifies that the rule detects when Domain depends on Persistence.
    /// </summary>
    [Fact]
    public void DomainLayer_WhenDependingOnPersistence_ShouldBeDetected()
    {
        // Arrange
        var domainNamespace = $"{FixturesCleanArchNamespace}.Domain";
        var persistenceNamespace = $"{FixturesCleanArchNamespace}.Persistence";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard
        var domainTypes = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(domainTypes, $"Domain namespace '{domainNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .ShouldNot()
            .HaveDependencyOn(persistenceNamespace)
            .GetResult();

        // Assert
        AssertRuleFails(result, "DomainLayer_ShouldNotDependOn_PersistenceLayer");
        AssertTypeInFailures(result, "DomainDependingOnPersistence");
    }

    /// <summary>
    /// Verifies that the rule detects when Domain depends on Presentation.
    /// </summary>
    [Fact]
    public void DomainLayer_WhenDependingOnPresentation_ShouldBeDetected()
    {
        // Arrange
        var domainNamespace = $"{FixturesCleanArchNamespace}.Domain";
        var presentationNamespace = $"{FixturesCleanArchNamespace}.Presentation";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard
        var domainTypes = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(domainTypes, $"Domain namespace '{domainNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(domainNamespace)
            .ShouldNot()
            .HaveDependencyOn(presentationNamespace)
            .GetResult();

        // Assert
        AssertRuleFails(result, "DomainLayer_ShouldNotDependOn_PresentationLayer");
        AssertTypeInFailures(result, "DomainDependingOnPresentation");
    }

    /// <summary>
    /// Verifies that the rule detects when Application depends on Presentation.
    /// </summary>
    [Fact]
    public void ApplicationLayer_WhenDependingOnPresentation_ShouldBeDetected()
    {
        // Arrange
        var applicationNamespace = $"{FixturesCleanArchNamespace}.Application";
        var presentationNamespace = $"{FixturesCleanArchNamespace}.Presentation";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard
        var applicationTypes = types
            .That()
            .ResideInNamespaceStartingWith(applicationNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(applicationTypes, $"Application namespace '{applicationNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(applicationNamespace)
            .ShouldNot()
            .HaveDependencyOn(presentationNamespace)
            .GetResult();

        // Assert
        AssertRuleFails(result, "ApplicationLayer_ShouldNotDependOn_PresentationLayer");
        AssertTypeInFailures(result, "ApplicationDependingOnPresentation");
    }

    /// <summary>
    /// Verifies that the rule detects when Application depends on Persistence implementations.
    /// </summary>
    [Fact]
    public void ApplicationLayer_WhenDependingOnPersistence_ShouldBeDetected()
    {
        // Arrange
        const string applicationNamespace = $"{FixturesCleanArchNamespace}.Application";
        const string persistenceNamespace = $"{FixturesCleanArchNamespace}.Persistence";
        var types = Types.InAssembly(ViolationFixturesAssembly);

        // Guard
        var applicationTypes = types
            .That()
            .ResideInNamespaceStartingWith(applicationNamespace)
            .GetTypes();
        GuardAgainstEmptyTypes(applicationTypes, $"Application namespace '{applicationNamespace}'");

        // Act
        var result = types
            .That()
            .ResideInNamespaceStartingWith(applicationNamespace)
            .ShouldNot()
            .HaveDependencyOn(persistenceNamespace)
            .GetResult();

        // Assert
        AssertRuleFails(result, "ApplicationLayer_ShouldNotDependOn_PersistenceImplementations");
        AssertTypeInFailures(result, "ApplicationDependingOnPersistence");
    }
}
