namespace FamilyHub.Api.Common.Configuration;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = "account";
    public string Issuer { get; set; } = string.Empty;
    public string? Issuers { get; set; }
}
