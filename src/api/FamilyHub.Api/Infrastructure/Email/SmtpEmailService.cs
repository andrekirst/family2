using System.Net;
using System.Net.Mail;
using FamilyHub.Api.Application.Services;

namespace FamilyHub.Api.Infrastructure.Email;

public sealed class EmailSettings
{
    public required string SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public string? SmtpUsername { get; init; }
    public string? SmtpPassword { get; init; }
    public required string FromAddress { get; init; }
    public required string FromName { get; init; }
    public bool EnableSsl { get; init; } = false;
}

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _settings = configuration.GetSection("Email").Get<EmailSettings>()
            ?? throw new InvalidOperationException("Email settings not configured");
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var verificationUrl = $"http://localhost:4200/verify-email?token={Uri.EscapeDataString(token)}";

        var subject = "Verify your email address";
        var body = $"""
            <h1>Welcome to Family Hub!</h1>
            <p>Please verify your email address by clicking the link below:</p>
            <p><a href="{verificationUrl}">Verify Email</a></p>
            <p>This link expires in 24 hours.</p>
            <p>If you didn't create an account, you can ignore this email.</p>
            """;

        await SendEmailAsync(toEmail, subject, body, ct);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var resetUrl = $"http://localhost:4200/reset-password?token={Uri.EscapeDataString(token)}";

        var subject = "Reset your password";
        var body = $"""
            <h1>Password Reset Request</h1>
            <p>Click the link below to reset your password:</p>
            <p><a href="{resetUrl}">Reset Password</a></p>
            <p>This link expires in 1 hour.</p>
            <p>If you didn't request a password reset, you can ignore this email.</p>
            """;

        await SendEmailAsync(toEmail, subject, body, ct);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
            {
                client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
            }

            client.EnableSsl = _settings.EnableSsl;

            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            throw;
        }
    }
}
