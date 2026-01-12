namespace FamilyHub.Tests.Architecture.Fixtures.Violations.DddPatterns.Domain.Aggregates;

/// <summary>
/// INTENTIONAL VIOLATION: Aggregate that does NOT inherit from AggregateRoot base.
/// Used for negative testing of DddPatternTests.AggregateRoots_ShouldInheritFrom_AggregateRootBase
/// </summary>
public sealed class BadAggregate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
