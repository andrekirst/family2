namespace FamilyHub.Api.Features.Family.Models;

/// <summary>
/// GraphQL input for accepting a family invitation.
/// </summary>
public class AcceptInvitationRequest
{
    public string Token { get; set; } = string.Empty;
}
