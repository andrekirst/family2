using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Result of querying user families.
/// </summary>
public sealed record GetUserFamiliesResult
{
    /// <summary>
    /// List of families the user belongs to.
    /// </summary>
    public required List<FamilyDto> Families { get; init; }
}

/// <summary>
/// DTO representing a family with basic information.
/// </summary>
public sealed record FamilyDto
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
    /// Number of active members in the family.
    /// </summary>
    public required int MemberCount { get; init; }
}
