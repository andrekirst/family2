namespace FamilyHub.Infrastructure.Email;

using MailKit.Net.Smtp;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

/// <summary>
/// Health check for SMTP connectivity.
/// </summary>
public sealed class SmtpHealthCheck : IHealthCheck
{
    private readonly SmtpSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpHealthCheck"/> class.
    /// </summary>
    /// <param name="settings">The SMTP configuration settings.</param>
    public SmtpHealthCheck(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseTls,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);
            }

            await client.DisconnectAsync(true, cancellationToken);

            return HealthCheckResult.Healthy("SMTP connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SMTP connection failed", ex);
        }
    }
}
