namespace FamilyHub.Modules.Auth.Infrastructure.Configuration;

/// <summary>
/// JWT configuration settings for local authentication.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Configuration section name for JWT settings.
    /// </summary>
    public const string SectionName = "Authentication:JwtSettings";

    /// <summary>
    /// Secret key for signing tokens (minimum 32 characters).
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer.
    /// </summary>
    public string Issuer { get; set; } = "https://familyhub.app";

    /// <summary>
    /// Token audience.
    /// </summary>
    public string Audience { get; set; } = "https://familyhub.app";

    /// <summary>
    /// Access token expiration in minutes (default: 15).
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration in days (default: 7).
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Validates that all required settings are configured.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SecretKey)
            && SecretKey.Length >= 32
            && !string.IsNullOrWhiteSpace(Issuer)
            && !string.IsNullOrWhiteSpace(Audience);
    }
}
