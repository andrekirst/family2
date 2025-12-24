namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

/// <summary>
/// GraphQL type representing a family.
/// </summary>
public sealed record FamilyType
{
    /// <summary>
    /// The unique identifier of the family.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The name of the family.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The ID of the user who owns this family.
    /// </summary>
    public required Guid OwnerId { get; init; }

    /// <summary>
    /// When the family was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the family was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Family members (optional, for expanded queries).
    /// </summary>
    public IReadOnlyCollection<UserFamilyType>? UserFamilies { get; init; }
}
