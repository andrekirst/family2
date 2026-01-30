namespace FamilyHub.Api.Application.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string token, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default);
}
