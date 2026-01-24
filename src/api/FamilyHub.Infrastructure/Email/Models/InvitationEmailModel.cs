namespace FamilyHub.Infrastructure.Email.Models;

/// <summary>
/// Model for invitation email template.
/// </summary>
public sealed record InvitationEmailModel
{
    /// <summary>
    /// Gets the name of the user who sent the invitation.
    /// </summary>
    public required string InviterName { get; init; }

    /// <summary>
    /// Gets the name of the family to which the user is being invited.
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Gets the URL to accept the invitation.
    /// </summary>
    public required string InvitationUrl { get; init; }

    /// <summary>
    /// Gets the date and time when the invitation expires.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets the role to which the user is being invited (e.g., "Member", "Admin").
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Gets the optional personal message from the inviter.
    /// </summary>
    public string? Message { get; init; }
}
