namespace FamilyHub.Api.Features.GoogleIntegration.Models;

public class GoogleIntegrationOptions
{
    public const string SectionName = "GoogleIntegration";

    public string EncryptionKey { get; set; } = string.Empty;
}
