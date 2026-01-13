using FamilyHub.Tests.Architecture.Helpers;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Negative tests for DDD Pattern rules.
/// These tests verify that DDD pattern rules actually DETECT violations
/// by testing against intentionally violating types in the fixtures assembly.
/// </summary>
public sealed class DddPatternNegativeTests : ArchitectureTestBase
{
    private const string FixturesDddNamespace =
        "FamilyHub.Tests.Architecture.Fixtures.Violations.DddPatterns";

    /// <summary>
    /// Verifies that the rule detects aggregates that don't inherit from AggregateRoot.
    /// </summary>
    [Fact]
    public void AggregateRoots_WhenNotInheritingBase_ShouldBeDetected()
    {
        // Arrange
        var aggregatesNamespace = $"{FixturesDddNamespace}.Domain.Aggregates";
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find types in Aggregates namespace
        var aggregateTypes = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Aggregates") &&
                        t.IsClass &&
                        !t.IsAbstract)
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(aggregateTypes, $"Aggregates namespace in fixtures");

        // Act - Check if any inherit from AggregateRoot (they shouldn't in our fixtures)
        var violatingTypes = aggregateTypes
            .Where(t => !InheritsFromAggregateRoot(t))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations (our fixtures intentionally don't inherit)
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain aggregates that violate the inheritance rule");
        violatingTypes.Should().Contain(t => t!.Contains("BadAggregate"),
            because: "BadAggregate fixture should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects domain events that don't inherit from DomainEvent base.
    /// </summary>
    [Fact]
    public void DomainEvents_WhenNotInheritingBase_ShouldBeDetected()
    {
        // Arrange
        var eventsNamespace = $"{FixturesDddNamespace}.Domain.Events";
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find types in Events namespace that should be domain events
        var eventTypes = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Events") &&
                        !t.IsAbstract &&
                        !t.IsInterface)
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(eventTypes, $"Events namespace in fixtures");

        // Act - Check if any don't inherit from DomainEvent
        var violatingTypes = eventTypes
            .Where(t => !typeof(FamilyHub.SharedKernel.Domain.DomainEvent).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain events that violate the inheritance rule");
        violatingTypes.Should().Contain(t => t!.Contains("BadDomainEvent"),
            because: "BadDomainEvent fixture should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects repository interfaces outside Domain.Repositories namespace.
    /// </summary>
    [Fact]
    public void RepositoryInterfaces_WhenInWrongNamespace_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find repository interfaces in fixtures (in wrong namespace)
        var repositoryInterfaces = fixtureTypes
            .Where(t => t.IsInterface &&
                        t.Name.StartsWith("I") &&
                        t.Name.Contains("Repository"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(repositoryInterfaces, "Repository interfaces in fixtures");

        // Act - Check if any are NOT in Domain.Repositories namespace
        var violatingTypes = repositoryInterfaces
            .Where(t => t.Namespace == null ||
                        !t.Namespace.Contains("Domain.Repositories"))
            .Select(t => $"{t.FullName} (Namespace: {t.Namespace})")
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain repository interfaces in wrong namespace");
        violatingTypes.Should().Contain(t => t.Contains("IBadRepository"),
            because: "IBadRepository fixture should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects repository implementations outside Persistence namespace.
    /// </summary>
    [Fact]
    public void RepositoryImplementations_WhenInWrongNamespace_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find repository implementations in fixtures
        var repositoryImplementations = fixtureTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.EndsWith("Repository") &&
                        !t.Name.StartsWith("I"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(repositoryImplementations, "Repository implementations in fixtures");

        // Act - Check if any are NOT in Persistence namespace
        var violatingTypes = repositoryImplementations
            .Where(t => t.Namespace == null ||
                        !t.Namespace.Contains("Persistence"))
            .Select(t => $"{t.FullName} (Namespace: {t.Namespace})")
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain repository implementations in wrong namespace");
        violatingTypes.Should().Contain(t => t.Contains("BadRepository"),
            because: "BadRepository fixture should be detected as a violation");
    }

    /// <summary>
    /// Helper method to check if a type inherits from AggregateRoot{TId}.
    /// </summary>
    private static bool InheritsFromAggregateRoot(Type type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(FamilyHub.SharedKernel.Domain.AggregateRoot<>))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }
}
