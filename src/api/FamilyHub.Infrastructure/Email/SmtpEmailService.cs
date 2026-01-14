namespace FamilyHub.Infrastructure.Email;

using FamilyHub.Infrastructure.Email.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.Retry;

/// <summary>
/// SMTP email service implementation using MimeKit.
/// </summary>
public sealed partial class SmtpEmailService : IEmailService, IAsyncDisposable
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly ResiliencePipeline _resiliencePipeline;
    private SmtpClient? _smtpClient;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailService"/> class.
    /// </summary>
    /// <param name="settings">The SMTP configuration settings.</param>
    /// <param name="logger">The logger instance.</param>
    public SmtpEmailService(
        IOptions<SmtpSettings> settings,
        ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Build Polly resilience pipeline (follows RabbitMqPublisher pattern)
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = _settings.MaxRetryAttempts,
                Delay = _settings.RetryBaseDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<SmtpCommandException>()
            })
            .Build();
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            LogSendingEmail(message.To);

            var mimeMessage = BuildMimeMessage(message);

            await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var client = await EnsureConnectedAsync(ct);
                await client.SendAsync(mimeMessage, ct);
            }, cancellationToken);

            LogEmailSent(message.To);
            return true;
        }
        catch (Exception ex)
        {
            LogEmailSendFailed(message.To, ex);
            return false;
        }
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_settings.FromDisplayName, _settings.FromAddress));
        mimeMessage.To.Add(new MailboxAddress(message.ToName, message.To));
        mimeMessage.Subject = message.Subject;

        if (!string.IsNullOrWhiteSpace(message.ReplyTo))
        {
            mimeMessage.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
        }

        var builder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody ?? string.Empty
        };

        mimeMessage.Body = builder.ToMessageBody();
        return mimeMessage;
    }

    private async Task<SmtpClient> EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_smtpClient?.IsConnected == true)
        {
            return _smtpClient;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_smtpClient?.IsConnected == true)
            {
                return _smtpClient;
            }

            _smtpClient?.Dispose();
            _smtpClient = new SmtpClient();

            LogConnectingToSmtp(_settings.Host, _settings.Port);

            await _smtpClient.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseTls,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.Username) &&
                !string.IsNullOrWhiteSpace(_settings.Password))
            {
                await _smtpClient.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);
            }

            LogConnectedToSmtp(_settings.Host, _settings.Port);
            return _smtpClient;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_smtpClient != null)
        {
            if (_smtpClient.IsConnected)
            {
                await _smtpClient.DisconnectAsync(true);
            }
            _smtpClient.Dispose();
        }
        _connectionLock.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Sending email to: {To}")]
    partial void LogSendingEmail(string to);

    [LoggerMessage(Level = LogLevel.Information, Message = "Email sent successfully to: {To}")]
    partial void LogEmailSent(string to);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send email to: {To}")]
    partial void LogEmailSendFailed(string to, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Connecting to SMTP server: {Host}:{Port}")]
    partial void LogConnectingToSmtp(string host, int port);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Connected to SMTP server: {Host}:{Port}")]
    partial void LogConnectedToSmtp(string host, int port);
}
