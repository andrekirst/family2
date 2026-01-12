namespace FamilyHub.Tests.Architecture.Fixtures.Violations.CqrsPatterns.Application.Queries;

/// <summary>
/// INTENTIONAL VIOLATION: Query class that does NOT implement IRequest.
/// Used for negative testing of CqrsPatternTests.Queries_ShouldImplement_IRequest
/// </summary>
public sealed record BadQuery
{
    public Guid Id { get; init; }
}
