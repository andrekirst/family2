namespace FamilyHub.Modules.Family.Domain;

using FamilyHub.Modules.Family.Domain.Enums;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.ValueObjects;

/// <summary>
/// Tracks email delivery attempts and status.
/// </summary>
public sealed class EmailOutbox : Entity<EmailOutboxId>
{
    /// <summary>
    /// Gets the associated outbox event identifier.
    /// </summary>
    public OutboxEventId OutboxEventId { get; private set; }

    /// <summary>
    /// Gets the recipient email address.
    /// </summary>
    public string To { get; private set; } = null!;

    /// <summary>
    /// Gets the recipient display name.
    /// </summary>
    public string ToName { get; private set; } = null!;

    /// <summary>
    /// Gets the email subject line.
    /// </summary>
    public string Subject { get; private set; } = null!;

    /// <summary>
    /// Gets the HTML body content of the email.
    /// </summary>
    public string HtmlBody { get; private set; } = null!;

    /// <summary>
    /// Gets the optional plain text body content of the email.
    /// </summary>
    public string? TextBody { get; private set; }

    /// <summary>
    /// Gets the current email delivery status.
    /// </summary>
    public EmailStatus Status { get; private set; }

    /// <summary>
    /// Gets the date and time when the email was successfully sent.
    /// </summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>
    /// Gets the date and time of the last delivery attempt.
    /// </summary>
    public DateTime? LastAttemptAt { get; private set; }

    /// <summary>
    /// Gets the number of delivery retry attempts.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Gets the error message from the most recent failed delivery attempt.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // Private constructor for EF Core
    private EmailOutbox() : base(EmailOutboxId.From(Guid.Empty))
    {
    }

    private EmailOutbox(EmailOutboxId id) : base(id)
    {
    }

    /// <summary>
    /// Creates a new email outbox entry for delivery tracking.
    /// </summary>
    /// <param name="outboxEventId">The associated outbox event identifier.</param>
    /// <param name="to">The recipient email address.</param>
    /// <param name="toName">The recipient display name.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML body content.</param>
    /// <param name="textBody">Optional plain text body content.</param>
    /// <returns>A new email outbox entry with pending status.</returns>
    public static EmailOutbox Create(
        OutboxEventId outboxEventId,
        string to,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null)
    {
        return new EmailOutbox(EmailOutboxId.New())
        {
            OutboxEventId = outboxEventId,
            To = to,
            ToName = toName,
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            Status = EmailStatus.Pending,
            RetryCount = 0
        };
    }

    /// <summary>
    /// Marks the email as successfully sent.
    /// </summary>
    public void MarkAsSent()
    {
        Status = EmailStatus.Sent;
        SentAt = DateTime.UtcNow;
        LastAttemptAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks the email as failed with retry capability.
    /// Increments retry count and marks as permanently failed if max retries exceeded.
    /// </summary>
    /// <param name="errorMessage">The error message from the failed delivery attempt.</param>
    public void MarkAsFailedWithRetry(string errorMessage)
    {
        RetryCount++;
        LastAttemptAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;

        if (RetryCount >= 10) // Max retries
        {
            Status = EmailStatus.PermanentlyFailed;
        }
        else
        {
            Status = EmailStatus.Failed;
        }
    }

    /// <summary>
    /// Determines whether the email is eligible for retry based on status, retry count, and exponential backoff timing.
    /// </summary>
    /// <returns>True if the email can be retried; otherwise, false.</returns>
    public bool CanRetry()
    {
        if (Status != EmailStatus.Failed) return false;
        if (RetryCount >= 10) return false;

        // Exponential backoff: 1m, 2m, 5m, 15m, 1h, 5h, 15h (caps at 15h)
        var delays = new[] { 1, 2, 5, 15, 60, 300, 900 };
        var delayMinutes = delays[Math.Min(RetryCount, delays.Length - 1)];
        var nextRetryTime = LastAttemptAt!.Value.AddMinutes(delayMinutes);

        return DateTime.UtcNow >= nextRetryTime;
    }
}
