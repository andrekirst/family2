namespace FamilyHub.Api.Features.Family.Models;

/// <summary>
/// GraphQL input for sending a family invitation.
/// </summary>
public class SendInvitationRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
}
