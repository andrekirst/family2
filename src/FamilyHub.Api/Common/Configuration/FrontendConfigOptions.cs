namespace FamilyHub.Api.Common.Configuration;

public sealed class FrontendConfigOptions
{
    public const string SectionName = "FrontendConfig";
    public string AppUrl { get; set; } = "http://localhost:4200";
    public string ApiUrl { get; set; } = "http://localhost:5152/graphql";
    public string KeycloakIssuer { get; set; } = "http://localhost:8080/realms/FamilyHub";
    public string KeycloakClientId { get; set; } = "familyhub-web";
}
