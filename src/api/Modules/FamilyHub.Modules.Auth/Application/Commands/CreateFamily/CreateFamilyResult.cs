using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Result of creating a new family.
/// Contains the created family information.
/// </summary>
public sealed record CreateFamilyResult
{
    /// <summary>
    /// The unique identifier of the created family.
    /// </summary>
    public required FamilyId FamilyId { get; init; }

    /// <summary>
    /// The name of the family.
    /// </summary>
    public required FamilyName Name { get; init; }

    /// <summary>
    /// The user ID of the family owner.
    /// </summary>
    public required UserId OwnerId { get; init; }

    /// <summary>
    /// When the family was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
