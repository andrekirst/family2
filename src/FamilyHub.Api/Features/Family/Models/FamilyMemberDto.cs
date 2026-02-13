namespace FamilyHub.Api.Features.Family.Models;

public class FamilyMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? AvatarId { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}
