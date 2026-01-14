namespace FamilyHub.Infrastructure.Email;

using FamilyHub.Infrastructure.Email.Models;

/// <summary>
/// Interface for sending emails via various providers.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if sent successfully, false otherwise.</returns>
    Task<bool> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
