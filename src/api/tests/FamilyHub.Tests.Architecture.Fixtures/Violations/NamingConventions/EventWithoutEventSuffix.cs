namespace FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions.Domain.Events;

/// <summary>
/// INTENTIONAL VIOLATION: Domain event without 'Event' suffix.
/// Used for negative testing of NamingConventionTests.DomainEvents_ShouldEndWith_Event
/// </summary>
public sealed record UserCreated
{
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}
