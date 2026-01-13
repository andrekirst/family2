namespace FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions.Presentation.GraphQL.Inputs;

/// <summary>
/// INTENTIONAL VIOLATION: GraphQL Input type without 'Input' suffix.
/// Used for negative testing of NamingConventionTests.GraphQLInputs_ShouldEndWith_Input
/// </summary>
public sealed record CreateUserData
{
    public string Name { get; init; } = string.Empty;
}
