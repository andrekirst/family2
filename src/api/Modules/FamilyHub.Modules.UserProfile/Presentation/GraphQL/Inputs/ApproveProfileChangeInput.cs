using System.ComponentModel.DataAnnotations;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input type for approving a profile change request.
/// </summary>
public sealed record ApproveProfileChangeInput
{
    /// <summary>
    /// The ID of the change request to approve.
    /// </summary>
    [Required]
    public required Guid RequestId { get; init; }
}
