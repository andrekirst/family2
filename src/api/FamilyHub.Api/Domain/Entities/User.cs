using System.Security.Cryptography;
using FamilyHub.Api.Domain.Base;
using FamilyHub.Api.Domain.Events;
using FamilyHub.Api.Domain.ValueObjects;

namespace FamilyHub.Api.Domain.Entities;

public class User : AggregateRoot<UserId>
{
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }

    // Email verification
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    // Password reset
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    // Lockout
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndTime { get; private set; }
    public bool IsLockedOut => LockoutEndTime.HasValue && LockoutEndTime > DateTime.UtcNow;

    // Timestamps
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    // EF Core constructor
    private User() : base(UserId.New())
    {
        Email = Email.From("temp@temp.com");
        PasswordHash = PasswordHash.From("temp");
    }

    private User(UserId id, Email email, PasswordHash passwordHash) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        EmailVerified = false;
        FailedLoginAttempts = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(Email email, PasswordHash passwordHash)
    {
        var user = new User(UserId.New(), email, passwordHash);
        user.GenerateEmailVerificationToken();

        user.RaiseDomainEvent(new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.EmailVerificationToken!));

        return user;
    }

    public void SetPassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        ClearPasswordResetToken();

        RaiseDomainEvent(new PasswordChangedEvent(Id));
    }

    public void RecordFailedLogin(int maxAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEndTime = DateTime.UtcNow.Add(lockoutDuration ?? TimeSpan.FromMinutes(15));
        }
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEndTime = null;

        RaiseDomainEvent(new UserLoggedInEvent(Id));
    }

    public void CheckAndClearExpiredLockout()
    {
        if (LockoutEndTime.HasValue && LockoutEndTime <= DateTime.UtcNow)
        {
            LockoutEndTime = null;
            FailedLoginAttempts = 0;
        }
    }

    public void GenerateEmailVerificationToken()
    {
        EmailVerificationToken = GenerateSecureToken();
        EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
    }

    public bool VerifyEmailWithToken(string token)
    {
        if (EmailVerified) return true;

        if (string.IsNullOrEmpty(EmailVerificationToken) ||
            EmailVerificationTokenExpiresAt == null ||
            EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            return false;

        if (!string.Equals(EmailVerificationToken, token, StringComparison.Ordinal))
            return false;

        EmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;

        RaiseDomainEvent(new UserEmailVerifiedEvent(Id));
        return true;
    }

    public void GeneratePasswordResetToken()
    {
        PasswordResetToken = GenerateSecureToken();
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        RaiseDomainEvent(new PasswordResetRequestedEvent(
            Id,
            Email,
            PasswordResetToken));
    }

    public bool ValidatePasswordResetToken(string token)
    {
        if (string.IsNullOrEmpty(PasswordResetToken) ||
            PasswordResetTokenExpiresAt == null ||
            PasswordResetTokenExpiresAt < DateTime.UtcNow)
            return false;

        return string.Equals(PasswordResetToken, token, StringComparison.Ordinal);
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
