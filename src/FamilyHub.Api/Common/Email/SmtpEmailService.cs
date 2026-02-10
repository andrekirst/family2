using FamilyHub.Api.Common.Infrastructure.Extensions;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FamilyHub.Api.Common.Email;

/// <summary>
/// MailKit-based SMTP email service implementation.
/// In development, sends to MailHog for local email capture.
/// </summary>
public partial class SmtpEmailService(IOptions<EmailConfiguration> config, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailConfiguration _config = config.Value;

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string textBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.FromConfig(_config));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_config.Host, _config.Port, _config.UseSsl, ct);

            if (!string.IsNullOrEmpty(_config.Username))
            {
                await client.AuthenticateAsync(_config.Username, _config.Password, ct);
            }

            await client.SendAsync(message, ct);
            LogEmailSentToToWithSubjectSubject(logger, to, subject);
        }
        finally
        {
            await client.DisconnectAsync(true, ct);
        }
    }

    [LoggerMessage(LogLevel.Information, "Email sent to {to} with subject '{subject}'")]
    static partial void LogEmailSentToToWithSubjectSubject(ILogger<SmtpEmailService> logger, string to, string subject);
}
