namespace FamilyHub.Api.Common.Configuration;

public sealed class AppOptions
{
    public const string SectionName = "App";
    public string FrontendUrl { get; set; } = "http://localhost:4200";
}
