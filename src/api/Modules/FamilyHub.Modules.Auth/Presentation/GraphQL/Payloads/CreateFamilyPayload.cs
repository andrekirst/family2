using FamilyHub.SharedKernel.Presentation.GraphQL;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for family creation.
/// </summary>
public sealed record CreateFamilyPayload : PayloadBase
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
    public CreateFamilyPayload(IReadOnlyList<UserError> errors) : base(errors)
    {
        Family = null;
    }
}

/// <summary>
/// DTO representing a newly created family (for CreateFamilyPayload).
/// </summary>
public sealed record CreatedFamilyDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required Guid OwnerId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
