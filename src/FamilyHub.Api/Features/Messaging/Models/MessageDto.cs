namespace FamilyHub.Api.Features.Messaging.Models;

/// <summary>
/// Data transfer object for Message entity.
/// Includes resolved sender information (name, avatar).
/// </summary>
public class MessageDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid? SenderAvatarId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
