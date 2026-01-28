using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Application.Queries.GetUserFamily;

/// <summary>
/// Result DTO for GetUserFamilyQuery.
/// Contains the user's family data or null if no family exists.
/// </summary>
public sealed record GetUserFamilyResult
{
    /// <summary>
    /// The unique identifier of the family.
    /// </summary>
    public required FamilyId FamilyId { get; init; }

    /// <summary>
    /// The name of the family.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The user ID of the family owner.
    /// </summary>
    public required UserId OwnerId { get; init; }

    /// <summary>
    /// When the family was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the family was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}
