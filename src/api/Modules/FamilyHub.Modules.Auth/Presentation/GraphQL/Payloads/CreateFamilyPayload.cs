using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for family creation.
/// </summary>
[Obsolete("Replaced by Hot Chocolate v14 Mutation Conventions. Remove after frontend migration.")]
public sealed record CreateFamilyPayload
{
    /// <summary>
    /// The created family (null if errors occurred).
    /// </summary>
    public CreatedFamilyDto? Family { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    /// <param name="family">The created family</param>
    public CreateFamilyPayload(CreatedFamilyDto family)
    {
        Family = family;
    }

    /// <summary>
    /// Constructor for error payload (called by factory).
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    public CreateFamilyPayload(IReadOnlyList<UserError> errors)
    {
        Family = null;
        Errors = errors;
    }

    /// <summary>
    /// List of errors that occurred during mutation execution.
    /// Null or empty when the mutation succeeded.
    /// </summary>
    public IReadOnlyList<UserError>? Errors { get; init; }
}

/// <summary>
/// DTO representing a newly created family (for CreateFamilyPayload).
/// </summary>
public sealed record CreatedFamilyDto
{
    /// <summary>
    /// Gets the unique identifier of the family.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the family.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the unique identifier of the family owner.
    /// </summary>
    public required Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the date and time when the family was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the date and time when the family was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}
