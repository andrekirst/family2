using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;
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
    public FamilyType? Family { get; init; }

    /// <summary>
    /// Constructor for successful payload (called by factory).
    /// </summary>
    /// <param name="family">The created family</param>
    public CreateFamilyPayload(FamilyType family)
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
