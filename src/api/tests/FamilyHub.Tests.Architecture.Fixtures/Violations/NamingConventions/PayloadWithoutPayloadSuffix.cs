namespace FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions.Presentation.GraphQL.Payloads;

/// <summary>
/// INTENTIONAL VIOLATION: GraphQL Payload type without 'Payload' suffix.
/// Used for negative testing of NamingConventionTests.GraphQLPayloads_ShouldEndWith_Payload
/// </summary>
public sealed record CreateUserResult
{
    public Guid UserId { get; init; }
    public bool Success { get; init; }
}
