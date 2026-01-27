using FamilyHub.Infrastructure.Email;
using FamilyHub.Infrastructure.Email.Models;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Service for sending authentication-related emails.
/// Uses the shared email infrastructure with auth-specific templates.
/// </summary>
public sealed partial class AuthEmailService : IAuthEmailService
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly AuthEmailSettings _settings;
    private readonly ILogger<AuthEmailService> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthEmailService.
    /// </summary>
    public AuthEmailService(
        IEmailService emailService,
        IEmailTemplateService templateService,
        IOptions<AuthEmailSettings> settings,
        ILogger<AuthEmailService> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SendVerificationEmailAsync(
        User user,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verificationUrl = $"{_settings.BaseUrl}/verify-email/{verificationToken}";

            var model = new EmailVerificationModel
            {
                Email = user.Email.Value,
                VerificationUrl = verificationUrl,
                ExpirationTime = _settings.VerificationTokenExpirationHours + " hours"
            };

            var htmlBody = await _templateService.RenderTemplateAsync(
                "EmailVerification",
                model,
                cancellationToken);

            var message = new EmailMessage
            {
                To = user.Email.Value,
                ToName = user.Email.Value.Split('@')[0],
                Subject = "Verify your Family Hub email address",
                HtmlBody = htmlBody,
                TextBody = $"Please verify your email by visiting: {verificationUrl}"
            };

            var result = await _emailService.SendEmailAsync(message, cancellationToken);

            if (result)
            {
                LogVerificationEmailSent(user.Email.Value);
            }
            else
            {
                LogVerificationEmailFailed(user.Email.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogVerificationEmailError(user.Email.Value, ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendPasswordResetLinkAsync(
        Email email,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resetUrl = $"{_settings.BaseUrl}/reset-password/{resetToken}";

            var model = new PasswordResetLinkModel
            {
                Email = email.Value,
                ResetUrl = resetUrl,
                ExpirationTime = _settings.PasswordResetTokenExpirationHours + " hours"
            };

            var htmlBody = await _templateService.RenderTemplateAsync(
                "PasswordResetLink",
                model,
                cancellationToken);

            var message = new EmailMessage
            {
                To = email.Value,
                ToName = email.Value.Split('@')[0],
                Subject = "Reset your Family Hub password",
                HtmlBody = htmlBody,
                TextBody = $"Reset your password by visiting: {resetUrl}"
            };

            var result = await _emailService.SendEmailAsync(message, cancellationToken);

            if (result)
            {
                LogPasswordResetLinkSent(email.Value);
            }
            else
            {
                LogPasswordResetLinkFailed(email.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogPasswordResetLinkError(email.Value, ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendPasswordResetCodeAsync(
        Email email,
        string resetCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new PasswordResetCodeModel
            {
                Email = email.Value,
                ResetCode = resetCode,
                ExpirationTime = _settings.PasswordResetCodeExpirationMinutes + " minutes"
            };

            var htmlBody = await _templateService.RenderTemplateAsync(
                "PasswordResetCode",
                model,
                cancellationToken);

            var message = new EmailMessage
            {
                To = email.Value,
                ToName = email.Value.Split('@')[0],
                Subject = "Your Family Hub password reset code",
                HtmlBody = htmlBody,
                TextBody = $"Your password reset code is: {resetCode}"
            };

            var result = await _emailService.SendEmailAsync(message, cancellationToken);

            if (result)
            {
                LogPasswordResetCodeSent(email.Value);
            }
            else
            {
                LogPasswordResetCodeFailed(email.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogPasswordResetCodeError(email.Value, ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendWelcomeEmailAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new WelcomeEmailModel
            {
                Email = user.Email.Value,
                AppUrl = _settings.BaseUrl
            };

            var htmlBody = await _templateService.RenderTemplateAsync(
                "Welcome",
                model,
                cancellationToken);

            var message = new EmailMessage
            {
                To = user.Email.Value,
                ToName = user.Email.Value.Split('@')[0],
                Subject = "Welcome to Family Hub!",
                HtmlBody = htmlBody,
                TextBody = $"Welcome to Family Hub! Visit us at: {_settings.BaseUrl}"
            };

            var result = await _emailService.SendEmailAsync(message, cancellationToken);

            if (result)
            {
                LogWelcomeEmailSent(user.Email.Value);
            }
            else
            {
                LogWelcomeEmailFailed(user.Email.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogWelcomeEmailError(user.Email.Value, ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendPasswordChangedAlertAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var model = new PasswordChangedAlertModel
            {
                Email = user.Email.Value,
                ChangedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ResetPasswordUrl = $"{_settings.BaseUrl}/forgot-password"
            };

            var htmlBody = await _templateService.RenderTemplateAsync(
                "PasswordChangedAlert",
                model,
                cancellationToken);

            var message = new EmailMessage
            {
                To = user.Email.Value,
                ToName = user.Email.Value.Split('@')[0],
                Subject = "Your Family Hub password was changed",
                HtmlBody = htmlBody,
                TextBody = "Your password was recently changed. If this wasn't you, please reset your password immediately."
            };

            var result = await _emailService.SendEmailAsync(message, cancellationToken);

            if (result)
            {
                LogPasswordChangedAlertSent(user.Email.Value);
            }
            else
            {
                LogPasswordChangedAlertFailed(user.Email.Value);
            }

            return result;
        }
        catch (Exception ex)
        {
            LogPasswordChangedAlertError(user.Email.Value, ex);
            return false;
        }
    }

    // Verification email logging
    [LoggerMessage(Level = LogLevel.Information, Message = "Verification email sent to: {Email}")]
    partial void LogVerificationEmailSent(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to send verification email to: {Email}")]
    partial void LogVerificationEmailFailed(string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error sending verification email to: {Email}")]
    partial void LogVerificationEmailError(string email, Exception exception);

    // Password reset link logging
    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset link sent to: {Email}")]
    partial void LogPasswordResetLinkSent(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to send password reset link to: {Email}")]
    partial void LogPasswordResetLinkFailed(string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error sending password reset link to: {Email}")]
    partial void LogPasswordResetLinkError(string email, Exception exception);

    // Password reset code logging
    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset code sent to: {Email}")]
    partial void LogPasswordResetCodeSent(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to send password reset code to: {Email}")]
    partial void LogPasswordResetCodeFailed(string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error sending password reset code to: {Email}")]
    partial void LogPasswordResetCodeError(string email, Exception exception);

    // Welcome email logging
    [LoggerMessage(Level = LogLevel.Information, Message = "Welcome email sent to: {Email}")]
    partial void LogWelcomeEmailSent(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to send welcome email to: {Email}")]
    partial void LogWelcomeEmailFailed(string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error sending welcome email to: {Email}")]
    partial void LogWelcomeEmailError(string email, Exception exception);

    // Password changed alert logging
    [LoggerMessage(Level = LogLevel.Information, Message = "Password changed alert sent to: {Email}")]
    partial void LogPasswordChangedAlertSent(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to send password changed alert to: {Email}")]
    partial void LogPasswordChangedAlertFailed(string email);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error sending password changed alert to: {Email}")]
    partial void LogPasswordChangedAlertError(string email, Exception exception);
}

/// <summary>
/// Configuration settings for authentication emails.
/// </summary>
public sealed class AuthEmailSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Authentication:Email";

    /// <summary>
    /// Base URL for the application (used in email links).
    /// </summary>
    public string BaseUrl { get; set; } = "https://familyhub.app";

    /// <summary>
    /// How long verification tokens are valid (in hours).
    /// </summary>
    public int VerificationTokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// How long password reset tokens are valid (in hours).
    /// </summary>
    public int PasswordResetTokenExpirationHours { get; set; } = 1;

    /// <summary>
    /// How long password reset codes are valid (in minutes).
    /// </summary>
    public int PasswordResetCodeExpirationMinutes { get; set; } = 15;
}
