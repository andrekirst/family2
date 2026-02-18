namespace FamilyHub.Api.Features.FileManagement.Models;

public class SecureNoteDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string EncryptedTitle { get; set; } = string.Empty;
    public string EncryptedContent { get; set; } = string.Empty;
    public string Iv { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string Sentinel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
