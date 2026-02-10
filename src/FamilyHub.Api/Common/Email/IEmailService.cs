namespace FamilyHub.Api.Common.Email;

/// <summary>
/// Service interface for sending emails.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, string textBody, CancellationToken ct = default);
}
