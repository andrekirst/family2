using System.ComponentModel.DataAnnotations;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input type for rejecting a profile change request.
/// </summary>
public sealed record RejectProfileChangeInput
{
    /// <summary>
    /// The ID of the change request to reject.
    /// </summary>
    [Required]
    public required Guid RequestId { get; init; }

    /// <summary>
    /// The reason for rejection (minimum 10 characters).
    /// </summary>
    [Required]
    [MinLength(10)]
    public required string Reason { get; init; }
}
