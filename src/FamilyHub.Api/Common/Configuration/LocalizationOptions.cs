namespace FamilyHub.Api.Common.Configuration;

/// <summary>
/// Configurable localization settings bound from appsettings.json.
/// Drives both ASP.NET RequestLocalization and custom middleware behavior.
/// </summary>
public sealed class LocalizationOptions
{
    public const string SectionName = "Localization";

    public string DefaultLocale { get; set; } = "en";
    public string[] SupportedLocales { get; set; } = ["en", "de"];
}
