using FamilyHub.Modules.Auth.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Application.Abstractions;

/// <summary>
/// Service for sending authentication-related emails.
/// Provides high-level methods for verification, password reset, and welcome emails.
/// </summary>
public interface IAuthEmailService
{
    /// <summary>
    /// Sends an email verification link to a newly registered user.
    /// </summary>
    /// <param name="user">The user to send the verification email to.</param>
    /// <param name="verificationToken">The email verification token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was sent successfully.</returns>
    Task<bool> SendVerificationEmailAsync(
        User user,
        string verificationToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset link for web flow.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="resetToken">The password reset token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was sent successfully.</returns>
    Task<bool> SendPasswordResetLinkAsync(
        Email email,
        string resetToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset code for mobile flow.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="resetCode">The 6-digit password reset code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was sent successfully.</returns>
    Task<bool> SendPasswordResetCodeAsync(
        Email email,
        string resetCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a welcome email after successful registration and verification.
    /// </summary>
    /// <param name="user">The verified user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was sent successfully.</returns>
    Task<bool> SendWelcomeEmailAsync(
        User user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a security alert email when password is changed.
    /// </summary>
    /// <param name="user">The user whose password was changed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was sent successfully.</returns>
    Task<bool> SendPasswordChangedAlertAsync(
        User user,
        CancellationToken cancellationToken = default);
}
