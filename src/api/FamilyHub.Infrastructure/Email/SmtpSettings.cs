namespace FamilyHub.Infrastructure.Email;

/// <summary>
/// SMTP email service configuration settings.
/// </summary>
public sealed class SmtpSettings
{
    /// <summary>
    /// Configuration section name for SMTP settings.
    /// </summary>
    public const string SectionName = "Smtp";

    /// <summary>
    /// SMTP server hostname.
    /// </summary>
    public string Host { get; init; } = "localhost";

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int Port { get; init; } = 587;

    /// <summary>
    /// Whether to use TLS/SSL encryption.
    /// </summary>
    public bool UseTls { get; init; } = true;

    /// <summary>
    /// SMTP authentication username.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// SMTP authentication password.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// From email address.
    /// </summary>
    public string FromAddress { get; init; } = "no-reply@familyhub.com";

    /// <summary>
    /// From display name.
    /// </summary>
    public string FromDisplayName { get; init; } = "Family Hub";

    /// <summary>
    /// Maximum retry attempts for failed sends.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Base delay between retry attempts.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Connection timeout for SMTP operations.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Validates the settings configuration.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Host)
            && Port > 0 && Port <= 65535
            && !string.IsNullOrWhiteSpace(FromAddress);
    }
}
