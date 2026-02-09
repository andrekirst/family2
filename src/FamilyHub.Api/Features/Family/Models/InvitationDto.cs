namespace FamilyHub.Api.Features.Family.Models;

public class InvitationDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public string InvitedByName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
