using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Auth.Domain;

/// <summary>
/// User aggregate root representing a registered user in the system.
/// Supports local email/password authentication with future social provider support.
/// </summary>
public class User : AggregateRoot<UserId>, ISoftDeletable
{
    #region Core Properties

    /// <summary>
    /// User's email address (unique identifier for login).
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Whether the email has been verified.
    /// Users must verify email before accessing protected features.
    /// </summary>
    public bool EmailVerified { get; private set; }

    /// <summary>
    /// When the email was verified (null if not verified).
    /// </summary>
    public DateTime? EmailVerifiedAt { get; private set; }

    /// <summary>
    /// The family this user belongs to.
    /// Auto-created as personal family on registration.
    /// </summary>
    public FamilyId FamilyId { get; private set; }

    /// <summary>
    /// User's role in the family.
    /// </summary>
    public FamilyRole Role { get; private set; }

    /// <summary>
    /// Soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    #endregion

    #region Password Authentication

    /// <summary>
    /// Argon2id hash of the user's password.
    /// Null if user only uses external authentication (future social login).
    /// </summary>
    public PasswordHash? PasswordHash { get; private set; }

    /// <summary>
    /// Number of consecutive failed login attempts.
    /// Reset to 0 after successful login or lockout expiry.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// When the account lockout ends.
    /// Null if account is not locked out.
    /// </summary>
    public DateTime? LockoutEndTime { get; private set; }

    /// <summary>
    /// Whether the account is currently locked out.
    /// </summary>
    public bool IsLockedOut => LockoutEndTime.HasValue && LockoutEndTime > DateTime.UtcNow;

    #endregion

    #region Email Verification

    /// <summary>
    /// Token for email verification (sent via email).
    /// </summary>
    public string? EmailVerificationToken { get; private set; }

    /// <summary>
    /// When the email verification token expires.
    /// </summary>
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    #endregion

    #region Password Reset

    /// <summary>
    /// Token for password reset via email link (web flow).
    /// </summary>
    public string? PasswordResetToken { get; private set; }

    /// <summary>
    /// When the password reset token expires (typically 1 hour).
    /// </summary>
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    /// <summary>
    /// 6-digit code for password reset (mobile flow).
    /// </summary>
    public string? PasswordResetCode { get; private set; }

    /// <summary>
    /// When the password reset code expires (typically 15 minutes).
    /// </summary>
    public DateTime? PasswordResetCodeExpiresAt { get; private set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// External login providers linked to this user (Google, Apple, etc.).
    /// </summary>
    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = new List<ExternalLogin>();

    /// <summary>
    /// Active refresh tokens for this user.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    #endregion

    #region Constructors

    // Private constructor for EF Core
    private User() : base(UserId.From(Guid.NewGuid()))
    {
        Email = Email.From("temp@temp.com"); // EF Core will set the actual value
        FamilyId = FamilyId.From(Guid.Empty); // EF Core will set the actual value
        Role = FamilyRole.Member;
    }

    private User(UserId id, Email email, PasswordHash passwordHash, FamilyId familyId, FamilyRole role) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FamilyId = familyId;
        Role = role;
        EmailVerified = false;
        FailedLoginAttempts = 0;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new user with email/password authentication.
    /// Email verification is required before accessing protected features.
    /// </summary>
    /// <param name="email">User's email address (must be unique).</param>
    /// <param name="passwordHash">Argon2id hash of the password.</param>
    /// <param name="familyId">The auto-created personal family ID.</param>
    public static User CreateWithPassword(Email email, PasswordHash passwordHash, FamilyId familyId)
    {
        var user = new User(UserId.New(), email, passwordHash, familyId, FamilyRole.Owner);
        user.GenerateEmailVerificationToken();
        return user;
    }

    #endregion

    #region Password Methods

    /// <summary>
    /// Sets or updates the user's password.
    /// </summary>
    public void SetPassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash;

        // Clear any pending password reset tokens
        ClearPasswordResetTokens();
    }

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    /// <param name="maxAttempts">Maximum allowed attempts before lockout.</param>
    /// <param name="lockoutDuration">Duration of lockout.</param>
    public void RecordFailedLogin(int maxAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEndTime = DateTime.UtcNow.Add(lockoutDuration ?? TimeSpan.FromMinutes(15));
        }
    }

    /// <summary>
    /// Resets the failed login attempt counter after successful login.
    /// </summary>
    public void ResetLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutEndTime = null;
    }

    /// <summary>
    /// Checks if lockout has expired and clears it if so.
    /// </summary>
    public void CheckAndClearExpiredLockout()
    {
        if (LockoutEndTime.HasValue && LockoutEndTime <= DateTime.UtcNow)
        {
            LockoutEndTime = null;
            FailedLoginAttempts = 0;
        }
    }

    #endregion

    #region Email Verification Methods

    /// <summary>
    /// Generates a new email verification token.
    /// Token expires in 24 hours.
    /// </summary>
    public void GenerateEmailVerificationToken()
    {
        EmailVerificationToken = GenerateSecureToken();
        EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
    }

    /// <summary>
    /// Verifies the email with the provided token.
    /// </summary>
    /// <param name="token">The verification token from the email.</param>
    /// <returns>True if verification succeeded, false otherwise.</returns>
    public bool VerifyEmailWithToken(string token)
    {
        if (EmailVerified)
        {
            return true; // Already verified
        }

        if (string.IsNullOrEmpty(EmailVerificationToken) ||
            EmailVerificationTokenExpiresAt == null ||
            EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            return false; // Token expired or not set
        }

        if (!string.Equals(EmailVerificationToken, token, StringComparison.Ordinal))
        {
            return false; // Token doesn't match
        }

        // Mark as verified
        EmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;

        return true;
    }

    /// <summary>
    /// Marks the email as verified (legacy method for compatibility).
    /// </summary>
    public void VerifyEmail()
    {
        if (EmailVerified)
        {
            return;
        }

        EmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
    }

    #endregion

    #region Password Reset Methods

    /// <summary>
    /// Generates a password reset token (for email link flow).
    /// Token expires in 1 hour.
    /// </summary>
    public void GeneratePasswordResetToken()
    {
        PasswordResetToken = GenerateSecureToken();
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
    }

    /// <summary>
    /// Generates a 6-digit password reset code (for mobile flow).
    /// Code expires in 15 minutes.
    /// </summary>
    public void GeneratePasswordResetCode()
    {
        PasswordResetCode = GenerateSixDigitCode();
        PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);
    }

    /// <summary>
    /// Validates a password reset token.
    /// </summary>
    public bool ValidatePasswordResetToken(string token)
    {
        if (string.IsNullOrEmpty(PasswordResetToken) ||
            PasswordResetTokenExpiresAt == null ||
            PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        return string.Equals(PasswordResetToken, token, StringComparison.Ordinal);
    }

    /// <summary>
    /// Validates a password reset code.
    /// </summary>
    public bool ValidatePasswordResetCode(string code)
    {
        if (string.IsNullOrEmpty(PasswordResetCode) ||
            PasswordResetCodeExpiresAt == null ||
            PasswordResetCodeExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        return string.Equals(PasswordResetCode, code, StringComparison.Ordinal);
    }

    /// <summary>
    /// Clears password reset tokens after successful reset.
    /// </summary>
    public void ClearPasswordResetTokens()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        PasswordResetCode = null;
        PasswordResetCodeExpiresAt = null;
    }

    #endregion

    #region Family Methods

    /// <summary>
    /// Updates the user's family association.
    /// </summary>
    public void UpdateFamily(FamilyId newFamilyId)
    {
        FamilyId = newFamilyId;
    }

    /// <summary>
    /// Gets the user's role in the given family.
    /// </summary>
    public FamilyRole GetRoleInFamily(FamilyAggregate family)
    {
        return family.OwnerId == Id ? FamilyRole.Owner : FamilyRole.Member;
    }

    /// <summary>
    /// Updates the user's role in their family.
    /// </summary>
    public void UpdateRole(FamilyRole newRole)
    {
        Role = newRole;
    }

    #endregion

    #region Delete Methods

    /// <summary>
    /// Soft deletes the user.
    /// </summary>
    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }

    #endregion

    #region Private Helpers

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static string GenerateSixDigitCode()
    {
        var bytes = new byte[4];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
        return number.ToString("D6");
    }

    #endregion
}
