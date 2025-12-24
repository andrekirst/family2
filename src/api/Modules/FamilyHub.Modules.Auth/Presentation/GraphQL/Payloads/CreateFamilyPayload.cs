using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// GraphQL payload for family creation.
/// </summary>
public sealed record CreateFamilyPayload
{
    /// <summary>
    /// The created family (null if errors occurred).
    /// </summary>
    public FamilyType? Family { get; init; }

    /// <summary>
    /// Errors that occurred during family creation (empty if successful).
    /// </summary>
    public UserError[] Errors { get; init; } = [];

    /// <summary>
    /// Creates a successful payload.
    /// </summary>
    public static CreateFamilyPayload Success(FamilyType family)
        => new() { Family = family };

    /// <summary>
    /// Creates a failure payload with errors.
    /// </summary>
    public static CreateFamilyPayload Failure(params UserError[] errors)
        => new() { Errors = errors };
}
