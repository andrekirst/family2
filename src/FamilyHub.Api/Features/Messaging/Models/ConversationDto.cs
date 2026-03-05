namespace FamilyHub.Api.Features.Messaging.Models;

/// <summary>
/// Data transfer object for Conversation entity.
/// </summary>
public class ConversationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? FolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ConversationMemberDto> Members { get; set; } = [];
}

/// <summary>
/// Data transfer object for ConversationMember entity.
/// </summary>
public class ConversationMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}
