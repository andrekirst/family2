namespace FamilyHub.Api.Features.Family.Models;

public class AcceptInvitationResultDto
{
    public Guid FamilyId { get; set; }
    public Guid FamilyMemberId { get; set; }
    public bool Success { get; set; }
}
