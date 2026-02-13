namespace FamilyHub.Api.Features.GoogleIntegration.Models;

public class GoogleOAuthOptions
{
    public const string SectionName = "GoogleIntegration:OAuth";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scopes { get; set; } = "openid email profile https://www.googleapis.com/auth/calendar.readonly https://www.googleapis.com/auth/calendar.events";
}
