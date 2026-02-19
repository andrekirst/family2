namespace FamilyHub.Api.Features.GoogleIntegration.Models;

public class LinkedAccountDto
{
    public string GoogleAccountId { get; set; } = string.Empty;
    public string GoogleEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string GrantedScopes { get; set; } = string.Empty;
    public DateTime? LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
