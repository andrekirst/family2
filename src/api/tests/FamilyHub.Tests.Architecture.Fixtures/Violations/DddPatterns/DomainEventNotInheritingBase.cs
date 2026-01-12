namespace FamilyHub.Tests.Architecture.Fixtures.Violations.DddPatterns.Domain.Events;

/// <summary>
/// INTENTIONAL VIOLATION: Domain event that does NOT inherit from DomainEvent base.
/// Used for negative testing of DddPatternTests.DomainEvents_ShouldInheritFrom_DomainEventBase
/// </summary>
public sealed record BadDomainEvent
{
    public Guid Id { get; init; }
    public DateTime OccurredAt { get; init; }
}
