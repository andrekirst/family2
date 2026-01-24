
using FamilyHub.Infrastructure.Email;
using FamilyHub.Infrastructure.Email.Models;
using FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Persistence.Repositories;
using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Family.Infrastructure.BackgroundServices;
/// <summary>
/// Background service that processes outbox events and sends invitation emails.
/// Follows the OutboxEventPublisher pattern.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvitationEmailService"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider for creating scoped dependencies.</param>
/// <param name="logger">The logger instance.</param>
public sealed partial class InvitationEmailService(
    IServiceProvider serviceProvider,
    ILogger<InvitationEmailService> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<InvitationEmailService> _logger = logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogServiceStarted();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEmailsAsync(stoppingToken);
                await ProcessFailedEmailsForRetryAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                LogProcessingError(ex);
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        LogServiceStopped();
    }

    private async Task ProcessPendingEmailsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var emailOutboxRepo = scope.ServiceProvider
            .GetRequiredService<IEmailOutboxRepository>();
        var emailService = scope.ServiceProvider
            .GetRequiredService<IEmailService>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var pendingEmails = await emailOutboxRepo
            .GetPendingEmailsAsync(BatchSize, cancellationToken);

        if (pendingEmails.Count == 0)
        {
            return;
        }

        LogProcessingEmails(pendingEmails.Count);

        foreach (var emailOutbox in pendingEmails)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await SendEmailAsync(
                emailOutbox,
                emailService,
                unitOfWork,
                cancellationToken);
        }
    }

    private async Task ProcessFailedEmailsForRetryAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var emailOutboxRepo = scope.ServiceProvider
            .GetRequiredService<IEmailOutboxRepository>();
        var emailService = scope.ServiceProvider
            .GetRequiredService<IEmailService>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var failedEmails = await emailOutboxRepo
            .GetFailedEmailsForRetryAsync(BatchSize, cancellationToken);

        var retriableEmails = failedEmails.Where(e => e.CanRetry()).ToList();

        if (retriableEmails.Count == 0)
        {
            return;
        }

        LogRetryingEmails(retriableEmails.Count);

        foreach (var emailOutbox in retriableEmails)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await SendEmailAsync(
                emailOutbox,
                emailService,
                unitOfWork,
                cancellationToken);
        }
    }

    private async Task SendEmailAsync(
        EmailOutbox emailOutbox,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        try
        {
            var emailMessage = new EmailMessage
            {
                To = emailOutbox.To,
                ToName = emailOutbox.ToName,
                Subject = emailOutbox.Subject,
                HtmlBody = emailOutbox.HtmlBody,
                TextBody = emailOutbox.TextBody
            };

            var success = await emailService.SendEmailAsync(emailMessage, cancellationToken);

            if (success)
            {
                emailOutbox.MarkAsSent();
                LogEmailSent(emailOutbox.To);
            }
            else
            {
                emailOutbox.MarkAsFailedWithRetry("Email sending failed");
                LogEmailFailed(emailOutbox.To);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            LogEmailError(emailOutbox.To, ex);
            emailOutbox.MarkAsFailedWithRetry(ex.Message);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "InvitationEmailService started")]
    partial void LogServiceStarted();

    [LoggerMessage(Level = LogLevel.Information, Message = "InvitationEmailService stopped")]
    partial void LogServiceStopped();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing {Count} pending emails")]
    partial void LogProcessingEmails(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrying {Count} failed emails")]
    partial void LogRetryingEmails(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Email sent successfully to: {To}")]
    partial void LogEmailSent(string to);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Email failed to send to: {To}")]
    partial void LogEmailFailed(string to);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error sending email to: {To}")]
    partial void LogEmailError(string to, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error in email processing loop")]
    partial void LogProcessingError(Exception exception);
}
