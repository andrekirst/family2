using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Authentication service orchestrating registration, login, password management.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IAuthAuditLogRepository _auditLogRepository;
    private readonly LockoutPolicyOptions _lockoutOptions;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthService.
    /// </summary>
    public AuthService(
        IPasswordService passwordService,
        ITokenService tokenService,
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IAuthAuditLogRepository auditLogRepository,
        IOptions<LockoutPolicyOptions> lockoutOptions,
        ILogger<AuthService> logger)
    {
        _passwordService = passwordService;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _familyRepository = familyRepository;
        _auditLogRepository = auditLogRepository;
        _lockoutOptions = lockoutOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validate password strength
        var strengthResult = _passwordService.ValidateStrength(request.Password);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PasswordTooWeak, string.Join(" ", strengthResult.Errors));
        }

        // Check if email already exists
        var email = Email.From(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser != null)
        {
            await LogAuditEventAsync(null, email, AuthAuditEventType.RegistrationFailed,
                request.IpAddress, request.DeviceInfo, false, "Email already exists", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.EmailAlreadyExists);
        }

        // Create personal family for the user
        var familyName = FamilyName.From($"{email.Value.Split('@')[0]}'s Family");
        var tempUserId = UserId.New(); // Temporary ID for family creation
        var family = FamilyAggregate.Create(familyName, tempUserId);

        // Hash password and create user
        var passwordHash = _passwordService.HashPassword(request.Password);
        var user = User.CreateWithPassword(email, passwordHash, family.Id);

        // Update family with actual user ID as owner
        family.TransferOwnership(user.Id);

        // Persist family and user
        await _familyRepository.AddAsync(family, cancellationToken);
        await _userRepository.AddAsync(user, cancellationToken);

        // Generate tokens
        var tokens = await _tokenService.GenerateTokenPairAsync(user, request.DeviceInfo, request.IpAddress, cancellationToken);

        // Log successful registration
        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.Registration,
            request.IpAddress, request.DeviceInfo, true, null, cancellationToken);

        _logger.LogInformation("User registered successfully: {Email}", email.Value);

        // TODO: Send verification email via IEmailService
        // await _emailService.SendVerificationEmailAsync(user, cancellationToken);

        var authenticatedUser = new AuthenticatedUser
        {
            FamilyId = user.FamilyId,
            EmailVerified = user.EmailVerified
        };

        return AuthResult.Succeeded(tokens, user.Id, email, authenticatedUser);
    }

    /// <inheritdoc />
    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = Email.From(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null)
        {
            // Log failed attempt but don't reveal if email exists
            await LogAuditEventAsync(null, email, AuthAuditEventType.FailedLogin,
                request.IpAddress, request.DeviceInfo, false, "User not found", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.InvalidCredentials);
        }

        // Check if account is locked
        user.CheckAndClearExpiredLockout();
        if (user.IsLockedOut)
        {
            await LogAuditEventAsync(user.Id, email, AuthAuditEventType.FailedLogin,
                request.IpAddress, request.DeviceInfo, false, "Account locked", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.AccountLocked);
        }

        // Verify password
        if (user.PasswordHash == null || !_passwordService.VerifyPassword(user.PasswordHash.Value, request.Password))
        {
            user.RecordFailedLogin(_lockoutOptions.MaxFailedAttempts,
                TimeSpan.FromMinutes(_lockoutOptions.LockoutDurationMinutes));
            await _userRepository.UpdateAsync(user, cancellationToken);

            var reason = user.IsLockedOut ? "Account locked after failed attempts" : "Invalid password";
            var eventType = user.IsLockedOut ? AuthAuditEventType.AccountLockout : AuthAuditEventType.FailedLogin;

            await LogAuditEventAsync(user.Id, email, eventType,
                request.IpAddress, request.DeviceInfo, false, reason, cancellationToken);

            if (user.IsLockedOut)
            {
                _logger.LogWarning("Account locked due to failed login attempts: {Email}", email.Value);
                return AuthResult.Failed(AuthErrorCode.AccountLocked);
            }

            return AuthResult.Failed(AuthErrorCode.InvalidCredentials);
        }

        // Reset failed login attempts on successful login
        user.ResetLoginAttempts();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var tokens = await _tokenService.GenerateTokenPairAsync(user, request.DeviceInfo, request.IpAddress, cancellationToken);

        // Log successful login
        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.Login,
            request.IpAddress, request.DeviceInfo, true, null, cancellationToken);

        _logger.LogInformation("User logged in successfully: {Email}", email.Value);

        var authenticatedUser = new AuthenticatedUser
        {
            FamilyId = user.FamilyId,
            EmailVerified = user.EmailVerified
        };

        return AuthResult.Succeeded(tokens, user.Id, email, authenticatedUser);
    }

    /// <inheritdoc />
    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var result = await _tokenService.RevokeTokenAsync(refreshToken, cancellationToken);
        if (result)
        {
            _logger.LogInformation("User logged out successfully");
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<int> LogoutAllDevicesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var count = await _tokenService.RevokeAllUserTokensAsync(userId, cancellationToken);
        _logger.LogInformation("User logged out from all devices. Sessions revoked: {Count}", count);
        return count;
    }

    /// <inheritdoc />
    public async Task<AuthResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return AuthResult.Failed(AuthErrorCode.UserNotFound);
        }

        // Verify current password
        if (user.PasswordHash == null || !_passwordService.VerifyPassword(user.PasswordHash.Value, request.CurrentPassword))
        {
            await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.PasswordChange,
                null, null, false, "Invalid current password", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.PasswordMismatch);
        }

        // Validate new password strength
        var strengthResult = _passwordService.ValidateStrength(request.NewPassword);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PasswordTooWeak, string.Join(" ", strengthResult.Errors));
        }

        // Update password
        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Revoke all refresh tokens (force re-login on all devices)
        await _tokenService.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.PasswordChange,
            null, null, true, null, cancellationToken);

        _logger.LogInformation("Password changed for user: {UserId}", user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var email = Email.From(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
        {
            _logger.LogDebug("Password reset requested for non-existent email: {Email}", request.Email);
            return AuthResult.Succeeded();
        }

        // Generate reset token or code
        if (request.UseMobileCode)
        {
            user.GeneratePasswordResetCode();
        }
        else
        {
            user.GeneratePasswordResetToken();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        // TODO: Send reset email via IEmailService
        // if (request.UseMobileCode)
        //     await _emailService.SendPasswordResetCodeAsync(user, cancellationToken);
        // else
        //     await _emailService.SendPasswordResetLinkAsync(user, cancellationToken);

        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.PasswordResetRequested,
            null, null, true, null, cancellationToken);

        _logger.LogInformation("Password reset requested for user: {Email}", email.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by reset token
        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
        if (user == null || !user.ValidatePasswordResetToken(request.Token))
        {
            return AuthResult.Failed(AuthErrorCode.InvalidToken);
        }

        // Validate new password strength
        var strengthResult = _passwordService.ValidateStrength(request.NewPassword);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PasswordTooWeak, string.Join(" ", strengthResult.Errors));
        }

        // Update password and clear reset tokens
        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Revoke all refresh tokens
        await _tokenService.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.PasswordReset,
            null, null, true, null, cancellationToken);

        _logger.LogInformation("Password reset completed for user: {UserId}", user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> ResetPasswordWithCodeAsync(ResetPasswordWithCodeRequest request, CancellationToken cancellationToken = default)
    {
        var email = Email.From(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user == null || !user.ValidatePasswordResetCode(request.Code))
        {
            return AuthResult.Failed(AuthErrorCode.InvalidCode);
        }

        // Validate new password strength
        var strengthResult = _passwordService.ValidateStrength(request.NewPassword);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PasswordTooWeak, string.Join(" ", strengthResult.Errors));
        }

        // Update password and clear reset tokens
        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Revoke all refresh tokens
        await _tokenService.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.PasswordReset,
            null, null, true, null, cancellationToken);

        _logger.LogInformation("Password reset with code completed for user: {UserId}", user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailVerificationTokenAsync(token, cancellationToken);
        if (user == null)
        {
            return AuthResult.Failed(AuthErrorCode.InvalidToken);
        }

        if (user.EmailVerified)
        {
            return AuthResult.Failed(AuthErrorCode.EmailAlreadyVerified);
        }

        var success = user.VerifyEmailWithToken(token);
        if (!success)
        {
            return AuthResult.Failed(AuthErrorCode.InvalidToken);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.EmailVerification,
            null, null, true, null, cancellationToken);

        _logger.LogInformation("Email verified for user: {UserId}", user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> ResendVerificationEmailAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return AuthResult.Failed(AuthErrorCode.UserNotFound);
        }

        if (user.EmailVerified)
        {
            return AuthResult.Failed(AuthErrorCode.EmailAlreadyVerified);
        }

        user.GenerateEmailVerificationToken();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // TODO: Send verification email via IEmailService
        // await _emailService.SendVerificationEmailAsync(user, cancellationToken);

        _logger.LogInformation("Verification email resent for user: {UserId}", userId.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Validate and rotate the refresh token
        var result = await _tokenService.RefreshTokensAsync(refreshToken, null, cancellationToken);
        if (result == null)
        {
            _logger.LogWarning("Token refresh failed: invalid or expired refresh token");
            return AuthResult.Failed(AuthErrorCode.InvalidToken);
        }

        // Get the user to include full details in the response
        var user = await _userRepository.GetByIdAsync(result.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Token refresh failed: user not found for ID {UserId}", result.UserId.Value);
            return AuthResult.Failed(AuthErrorCode.UserNotFound);
        }

        var authenticatedUser = new AuthenticatedUser
        {
            FamilyId = user.FamilyId,
            EmailVerified = user.EmailVerified
        };

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.TokenRefresh,
            null, null, true, null, cancellationToken);

        _logger.LogDebug("Token refreshed for user: {UserId}", user.Id.Value);

        return AuthResult.Succeeded(result.Tokens, user.Id, user.Email, authenticatedUser);
    }

    private async Task LogAuditEventAsync(
        UserId? userId,
        Email? email,
        AuthAuditEventType eventType,
        string? ipAddress,
        string? userAgent,
        bool success,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        var auditLog = AuthAuditLog.Create(
            userId,
            email,
            eventType,
            ipAddress,
            userAgent,
            success,
            failureReason);

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
    }
}

/// <summary>
/// Lockout policy configuration options.
/// </summary>
public sealed class LockoutPolicyOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Authentication:LockoutPolicy";

    /// <summary>
    /// Maximum failed login attempts before lockout (default: 5).
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Lockout duration in minutes (default: 15).
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 15;
}
