namespace FamilyHub.Api.Features.FileManagement.Models;

public class ExternalConnectionDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsTokenExpired { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public Guid ConnectedBy { get; set; }
    public DateTime ConnectedAt { get; set; }
}
