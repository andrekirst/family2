namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for creating a new family.
/// </summary>
public sealed record CreateFamilyInput
{
    /// <summary>
    /// The name of the family (e.g., "Smith Family").
    /// </summary>
    public required string Name { get; init; }
}
