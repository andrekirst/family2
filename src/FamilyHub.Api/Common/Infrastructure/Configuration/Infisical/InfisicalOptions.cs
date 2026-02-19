namespace FamilyHub.Api.Common.Infrastructure.Configuration.Infisical;

public sealed class InfisicalOptions
{
    public string Url { get; set; } = "http://localhost:8180";
    public string ProjectId { get; set; } = "";
    public string Environment { get; set; } = "dev";
    public string SecretPath { get; set; } = "/";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
}
