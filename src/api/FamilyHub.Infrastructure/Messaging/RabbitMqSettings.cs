namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Configuration settings for RabbitMQ connection and behavior.
/// </summary>
/// <remarks>
/// Bind this class to the "RabbitMQ" section in appsettings.json.
/// Example configuration:
/// <code>
/// {
///   "RabbitMQ": {
///     "Host": "localhost",
///     "Port": 5672,
///     "Username": "familyhub",
///     "Password": "secret"
///   }
/// }
/// </code>
/// </remarks>
public sealed class RabbitMqSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ server hostname or IP address.
    /// Default: localhost
    /// </summary>
    public string Host { get; init; } = "localhost";

    /// <summary>
    /// AMQP port number.
    /// Default: 5672
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string Username { get; init; } = "guest";

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string Password { get; init; } = "guest";

    /// <summary>
    /// Virtual host to connect to.
    /// Default: /
    /// </summary>
    public string VirtualHost { get; init; } = "/";

    /// <summary>
    /// Client-provided connection name for identification in RabbitMQ Management UI.
    /// </summary>
    public string ClientProvidedName { get; init; } = "FamilyHub.Api";

    /// <summary>
    /// Default exchange for domain events.
    /// </summary>
    public string DefaultExchange { get; init; } = "family-hub.events";

    /// <summary>
    /// Dead letter exchange name for failed messages.
    /// </summary>
    public string DeadLetterExchange { get; init; } = "family-hub.dlx";

    /// <summary>
    /// Dead letter queue name.
    /// </summary>
    public string DeadLetterQueue { get; init; } = "family-hub.dlq";

    /// <summary>
    /// Maximum retry attempts for publish operations.
    /// Default: 3
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Initial delay between retries (exponential backoff base).
    /// Default: 1 second
    /// </summary>
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between retries.
    /// Default: 30 seconds
    /// </summary>
    public TimeSpan RetryMaxDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Connection timeout.
    /// Default: 30 seconds
    /// </summary>
    public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Enable publisher confirms for guaranteed delivery.
    /// Default: true
    /// </summary>
    public bool EnablePublisherConfirms { get; init; } = true;

    /// <summary>
    /// Validates that required settings are configured.
    /// </summary>
    /// <returns>True if all required settings are valid.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Host)
            && Port > 0
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrWhiteSpace(Password)
            && MaxRetryAttempts >= 0;
    }
}
