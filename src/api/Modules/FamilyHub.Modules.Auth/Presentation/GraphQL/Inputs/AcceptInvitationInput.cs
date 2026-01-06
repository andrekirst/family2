namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for accepting a family invitation.
/// Used by invitees to join a family via invitation token.
/// </summary>
public sealed record AcceptInvitationInput
{
    /// <summary>
    /// Invitation token (64-character URL-safe base64 string).
    /// Received via email invitation link.
    /// </summary>
    public required string Token { get; init; }
}
