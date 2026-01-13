using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// ASP.NET Core health check for RabbitMQ connectivity.
/// </summary>
/// <remarks>
/// <para>
/// Creates a test connection to verify RabbitMQ is available and responsive.
/// Uses a short timeout (5 seconds) for fast failure detection.
/// </para>
/// <para>
/// Returns server properties (product, version) in health check data
/// when connection is successful.
/// </para>
/// </remarks>
public sealed partial class RabbitMqHealthCheck : IHealthCheck
{
    private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);

    private readonly ILogger<RabbitMqHealthCheck> _logger;
    private readonly RabbitMqSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    /// <param name="settings">RabbitMQ configuration settings.</param>
    public RabbitMqHealthCheck(
        ILogger<RabbitMqHealthCheck> logger,
        IOptions<RabbitMqSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                RequestedConnectionTimeout = HealthCheckTimeout
            };

            await using var connection = await factory.CreateConnectionAsync(
                "FamilyHub.HealthCheck",
                cancellationToken);

            if (connection.ServerProperties != null)
            {
                var serverProperties = ExtractServerProperties(connection.ServerProperties);

                var data = new Dictionary<string, object>
                {
                    ["host"] = _settings.Host,
                    ["port"] = _settings.Port,
                    ["virtualHost"] = _settings.VirtualHost,
                    ["serverProduct"] = serverProperties.Product,
                    ["serverVersion"] = serverProperties.Version
                };

                LogHealthCheckPassed(_settings.Host, _settings.Port);

                return HealthCheckResult.Healthy(
                    "RabbitMQ connection successful",
                    data);
            }
        }
        catch (Exception ex)
        {
            LogHealthCheckFailed(_settings.Host, _settings.Port, ex.Message);

            return HealthCheckResult.Unhealthy(
                "RabbitMQ connection failed",
                ex,
                new Dictionary<string, object>
                {
                    ["host"] = _settings.Host,
                    ["port"] = _settings.Port,
                    ["error"] = ex.Message
                });
        }

        return HealthCheckResult.Unhealthy(
            "RabbitMQ connection failed",
            data: new Dictionary<string, object>
            {
                ["host"] = _settings.Host,
                ["port"] = _settings.Port,
                ["error"] = "Unknown error"
            });
    }

    /// <summary>
    /// Extracts product and version from RabbitMQ server properties.
    /// </summary>
    private static (string Product, string Version) ExtractServerProperties(
        IDictionary<string, object?> serverProperties)
    {
        var product = serverProperties.TryGetValue("product", out var productValue)
            ? ConvertToString(productValue)
            : "Unknown";

        var version = serverProperties.TryGetValue("version", out var versionValue)
            ? ConvertToString(versionValue)
            : "Unknown";

        return (product, version);
    }

    /// <summary>
    /// Converts server property value to string, handling byte arrays from AMQP.
    /// </summary>
    private static string ConvertToString(object? value)
    {
        return value switch
        {
            byte[] bytes => System.Text.Encoding.UTF8.GetString(bytes),
            string str => str,
            _ => value?.ToString() ?? "Unknown"
        };
    }

    [LoggerMessage(LogLevel.Debug, "RabbitMQ health check passed for {Host}:{Port}")]
    partial void LogHealthCheckPassed(string host, int port);

    [LoggerMessage(LogLevel.Warning, "RabbitMQ health check failed for {Host}:{Port}: {ErrorMessage}")]
    partial void LogHealthCheckFailed(string host, int port, string errorMessage);
}
