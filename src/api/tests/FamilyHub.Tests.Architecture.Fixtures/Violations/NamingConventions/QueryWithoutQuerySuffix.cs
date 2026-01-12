using MediatR;

namespace FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions.Application.Queries;

/// <summary>
/// INTENTIONAL VIOLATION: Query class without 'Query' suffix.
/// Used for negative testing of NamingConventionTests.Queries_ShouldEndWith_Query
/// </summary>
public sealed record GetUser : IRequest<object?>
{
    public Guid Id { get; init; }
}
