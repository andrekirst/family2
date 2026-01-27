using System.Reflection;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Architecture tests ensuring proper Domain-Driven Design patterns.
/// Validates aggregate roots, domain events, and repository patterns.
/// </summary>
public sealed class DddPatternTests
{
    private static readonly Assembly AuthModuleAssembly = typeof(CreateFamilyCommand).Assembly;
    private static readonly Assembly FamilyModuleAssembly = typeof(Family).Assembly;

    private static readonly Assembly[] AllModuleAssemblies =
    [
        AuthModuleAssembly,
        FamilyModuleAssembly
    ];

    /// <summary>
    /// Classes in Aggregates namespaces should inherit from AggregateRoot base class.
    /// Aggregate roots are the consistency boundaries in DDD.
    /// </summary>
    [Fact]
    public void AggregateRoots_ShouldInheritFrom_AggregateRootBase()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Aggregates namespaces (excluding generated/migration files)
        var aggregateTypes = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains(TestConstants.AggregatesSuffix.TrimStart('.')) &&
                        t is { IsClass: true, IsAbstract: false } &&
                        !t.Name.Contains("Designer") &&
                        !t.Name.Contains("Snapshot"))
            .ToList();

        // Act & Assert
        var violatingTypes = aggregateTypes
            .Where(t => !InheritsFromAggregateRoot(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All classes in .Aggregates namespace should inherit from AggregateRoot<TId>. " +
                     $"AggregateRoot provides domain event support and ensures proper DDD patterns. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Domain events should inherit from DomainEvent base class.
    /// This ensures proper event handling infrastructure support.
    /// </summary>
    [Fact]
    public void DomainEvents_ShouldInheritFrom_DomainEventBase()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Events namespaces
        var eventTypes = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains(TestConstants.EventsSuffix.TrimStart('.')) &&
                        t is { IsClass: true, IsAbstract: false } &&
                        t.Name.EndsWith("Event"))
            .ToList();

        // Act & Assert
        var violatingTypes = eventTypes
            .Where(t => !typeof(DomainEvent).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All domain event classes should inherit from DomainEvent base class. " +
                     $"This ensures proper event infrastructure support (EventId, OccurredOn, MediatR integration). " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Repository interfaces should reside in Domain.Repositories namespace.
    /// Following DDD, repository interfaces are part of the domain model.
    /// </summary>
    [Fact]
    public void RepositoryInterfaces_ShouldResideIn_DomainRepositoriesNamespace()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find all repository interfaces
        var repositoryInterfaces = allTypes
            .Where(t => t.IsInterface &&
                        t.Name.StartsWith("I") &&
                        t.Name.EndsWith("Repository"))
            .ToList();

        // Act & Assert
        var violatingTypes = repositoryInterfaces
            .Where(t => t.Namespace == null ||
                        !t.Namespace.Contains($"{TestConstants.DomainLayer.TrimStart('.')}{TestConstants.RepositoriesSuffix}"))
            .Select(t => $"{t.FullName} (Namespace: {t.Namespace})")
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"Repository interfaces should be defined in .Domain.Repositories namespace. " +
                     $"In DDD, repository interfaces are part of the domain model, " +
                     $"while implementations reside in the persistence layer. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Repository implementations should reside in Persistence layer.
    /// Concrete implementations are infrastructure concerns.
    /// </summary>
    [Fact]
    public void RepositoryImplementations_ShouldResideIn_PersistenceLayer()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find all repository implementation classes
        var repositoryImplementations = allTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.EndsWith("Repository") &&
                        !t.Name.StartsWith("I"))
            .ToList();

        // Act & Assert
        var violatingTypes = repositoryImplementations
            .Where(t => t.Namespace == null ||
                        !t.Namespace.Contains(TestConstants.PersistenceLayer.TrimStart('.')))
            .Select(t => $"{t.FullName} (Namespace: {t.Namespace})")
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"Repository implementations should reside in .Persistence namespace. " +
                     $"This separates infrastructure concerns from domain logic. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
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
                current.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }
}
