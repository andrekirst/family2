namespace FamilyHub.Modules.Auth.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for E2E test mode.
/// Enables header-based authentication for Playwright E2E tests.
/// </summary>
/// <remarks>
/// SECURITY: Test mode MUST NOT be enabled in Production environment.
/// The API will fail to start if TestMode is enabled with ASPNETCORE_ENVIRONMENT=Production.
/// </remarks>
public sealed class TestModeSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "TestMode";

    /// <summary>
    /// Environment variable name for enabling test mode.
    /// Alternative to appsettings configuration.
    /// </summary>
    public const string EnvironmentVariable = "FAMILYHUB_TEST_MODE";

    /// <summary>
    /// Whether test mode is enabled.
    /// When true, uses HeaderBasedCurrentUserService instead of JWT-based authentication.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Validates that test mode is not enabled in production.
    /// </summary>
    /// <param name="environmentName">Current ASP.NET Core environment name.</param>
    /// <exception cref="InvalidOperationException">Thrown if test mode is enabled in Production.</exception>
    public void ValidateNotProduction(string environmentName)
    {
        if (Enabled && environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "SECURITY VIOLATION: Test mode cannot be enabled in Production environment. " +
                $"Remove the {EnvironmentVariable} environment variable or set TestMode:Enabled to false.");
        }
    }
}
