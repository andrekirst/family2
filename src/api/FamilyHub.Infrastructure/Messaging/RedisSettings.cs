namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// Configuration settings for Redis connection and PubSub channels.
/// </summary>
/// <remarks>
/// Bind this class to the "Redis" section in appsettings.json.
/// Used for Hot Chocolate GraphQL subscriptions transport.
/// Example configuration:
/// <code>
/// {
///   "Redis": {
///     "ConnectionString": "localhost:6379",
///     "InstanceName": "FamilyHub:",
///     "Channels": {
///       "FamilyMembers": "family-members-changed",
///       "PendingInvitations": "pending-invitations-changed"
///     }
///   }
/// }
/// </code>
/// </remarks>
public sealed class RedisSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis connection string (format: "localhost:6379" or "host:port,password=secret").
    /// Default: localhost:6379
    /// </summary>
    public string ConnectionString { get; init; } = "localhost:6379";

    /// <summary>
    /// Instance name prefix for Redis keys (prevents collisions in shared Redis).
    /// Default: FamilyHub:
    /// </summary>
    public string InstanceName { get; init; } = "FamilyHub:";

    /// <summary>
    /// PubSub channel names for GraphQL subscriptions.
    /// </summary>
    public ChannelConfiguration Channels { get; init; } = new();

    /// <summary>
    /// Configuration timeout for Redis operations.
    /// Default: 30 seconds
    /// </summary>
    public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to abort on connection failure.
    /// Default: false (auto-reconnect)
    /// </summary>
    public bool AbortOnConnectFail { get; init; } = false;

    /// <summary>
    /// Validates that required settings are configured.
    /// </summary>
    /// <returns>True if all required settings are valid.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ConnectionString)
            && !string.IsNullOrWhiteSpace(InstanceName);
    }
}

/// <summary>
/// Redis PubSub channel configuration for GraphQL subscriptions.
/// </summary>
public sealed class ChannelConfiguration
{
    /// <summary>
    /// Channel for family members changes (ADDED, UPDATED, REMOVED).
    /// Default: family-members-changed
    /// </summary>
    public string FamilyMembers { get; init; } = "family-members-changed";

    /// <summary>
    /// Channel for pending invitations changes (ADDED, UPDATED, REMOVED).
    /// Default: pending-invitations-changed
    /// </summary>
    public string PendingInvitations { get; init; } = "pending-invitations-changed";
}
