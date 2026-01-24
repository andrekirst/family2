
using MailKit.Net.Smtp;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FamilyHub.Infrastructure.Email;
/// <summary>
/// Health check for SMTP connectivity.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SmtpHealthCheck"/> class.
/// </remarks>
/// <param name="settings">The SMTP configuration settings.</param>
public sealed class SmtpHealthCheck(IOptions<SmtpSettings> settings) : IHealthCheck
{
    private readonly SmtpSettings _settings = settings.Value;

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
