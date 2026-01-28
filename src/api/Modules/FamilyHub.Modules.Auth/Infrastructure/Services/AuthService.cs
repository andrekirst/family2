using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.Specifications;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Authentication service orchestrating registration, login, password management.
/// </summary>
public sealed partial class AuthService : IAuthService
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
    [Obsolete("Obsolete")]
    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validate password strength
        var strengthResult = _passwordService.ValidateStrength(request.Password);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PASSWORD_TOO_WEAK, string.Join(" ", strengthResult.Errors));
        }

        // Check if email already exists
        var email = Email.From(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser != null)
        {
            await LogAuditEventAsync(null, email, AuthAuditEventType.REGISTRATION_FAILED,
                request.IpAddress, request.DeviceInfo, false, "Email already exists", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.EMAIL_ALREADY_EXISTS);
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
        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.REGISTRATION,
            request.IpAddress, request.DeviceInfo, true, null, cancellationToken);

        LogUserRegisteredSuccessfullyEmail(email.Value);

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
        var specification = new UserByEmailSpecification(email);
        var user = await _userRepository.FindOneAsync(specification, cancellationToken);

        if (user == null)
        {
            // Log failed attempt but don't reveal if email exists
            await LogAuditEventAsync(null, email, AuthAuditEventType.FAILED_LOGIN,
                request.IpAddress, request.DeviceInfo, false, "User not found", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.INVALID_CREDENTIALS);
        }

        // Check if account is locked
        user.CheckAndClearExpiredLockout();
        if (user.IsLockedOut)
        {
            await LogAuditEventAsync(user.Id, email, AuthAuditEventType.FAILED_LOGIN,
                request.IpAddress, request.DeviceInfo, false, "Account locked", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.ACCOUNT_LOCKED);
        }

        // Verify password
        if (user.PasswordHash == null || !_passwordService.VerifyPassword(user.PasswordHash.Value, request.Password))
        {
            user.RecordFailedLogin(_lockoutOptions.MaxFailedAttempts,
                TimeSpan.FromMinutes(_lockoutOptions.LockoutDurationMinutes));
            await _userRepository.UpdateAsync(user, cancellationToken);

            var reason = user.IsLockedOut ? "Account locked after failed attempts" : "Invalid password";
            var eventType = user.IsLockedOut ? AuthAuditEventType.ACCOUNT_LOCKOUT : AuthAuditEventType.FAILED_LOGIN;

            await LogAuditEventAsync(user.Id, email, eventType,
                request.IpAddress, request.DeviceInfo, false, reason, cancellationToken);

            if (user.IsLockedOut)
            {
                LogAccountLockedDueToFailedLoginAttemptsEmail(email.Value);
                return AuthResult.Failed(AuthErrorCode.ACCOUNT_LOCKED);
            }

            return AuthResult.Failed(AuthErrorCode.INVALID_CREDENTIALS);
        }

        // Reset failed login attempts on successful login
        user.ResetLoginAttempts();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var tokens = await _tokenService.GenerateTokenPairAsync(user, request.DeviceInfo, request.IpAddress, cancellationToken);

        // Log successful login
        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.LOGIN,
            request.IpAddress, request.DeviceInfo, true, null, cancellationToken);

        LogUserLoggedInSuccessfullyEmail(email.Value);

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
            LogUserLoggedOutSuccessfully();
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<int> LogoutAllDevicesAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var count = await _tokenService.RevokeAllUserTokensAsync(userId, cancellationToken);
        LogUserLoggedOutFromAllDevicesSessionsRevokedCount(count);
        return count;
    }

    /// <inheritdoc />
    public async Task<AuthResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return AuthResult.Failed(AuthErrorCode.USER_NOT_FOUND);
        }

        // Verify current password
        if (user.PasswordHash == null || !_passwordService.VerifyPassword(user.PasswordHash.Value, request.CurrentPassword))
        {
            await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.PASSWORD_CHANGE,
                null, null, false, "Invalid current password", cancellationToken);
            return AuthResult.Failed(AuthErrorCode.PASSWORD_MISMATCH);
        }

        // Validate new password strength
        var strengthResult = _passwordService.ValidateStrength(request.NewPassword);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PASSWORD_TOO_WEAK, string.Join(" ", strengthResult.Errors));
        }

        // Update password
        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Revoke all refresh tokens (force re-login on all devices)
        await _tokenService.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.PASSWORD_CHANGE,
            null, null, true, null, cancellationToken);

        LogPasswordChangedForUserUserid(user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var email = Email.From(request.Email);
        var specification = new UserByEmailSpecification(email);
        var user = await _userRepository.FindOneAsync(specification, cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
        {
            LogPasswordResetRequestedForNonExistentEmailEmail(request.Email);
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

        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.PASSWORD_RESET_REQUESTED,
            null, null, true, null, cancellationToken);

        LogPasswordResetRequestedForUserEmail(email.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by reset token
        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
        if (user == null || !user.ValidatePasswordResetToken(request.Token))
        {
            return AuthResult.Failed(AuthErrorCode.INVALID_TOKEN);
        }

        // Validate new password strength
        var strengthResult = _passwordService.ValidateStrength(request.NewPassword);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PASSWORD_TOO_WEAK, string.Join(" ", strengthResult.Errors));
        }

        // Update password and clear reset tokens
        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Revoke all refresh tokens
        await _tokenService.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.PASSWORD_RESET,
            null, null, true, null, cancellationToken);

        LogPasswordResetCompletedForUserUserid(user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> ResetPasswordWithCodeAsync(ResetPasswordWithCodeRequest request, CancellationToken cancellationToken = default)
    {
        var email = Email.From(request.Email);
        var specification = new UserByEmailSpecification(email);
        var user = await _userRepository.FindOneAsync(specification, cancellationToken);

        if (user == null || !user.ValidatePasswordResetCode(request.Code))
        {
            return AuthResult.Failed(AuthErrorCode.INVALID_CODE);
        }

        // Validate new password strength
        var strengthResult = _passwordService.ValidateStrength(request.NewPassword);
        if (!strengthResult.IsValid)
        {
            return AuthResult.Failed(AuthErrorCode.PASSWORD_TOO_WEAK, string.Join(" ", strengthResult.Errors));
        }

        // Update password and clear reset tokens
        var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.SetPassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Revoke all refresh tokens
        await _tokenService.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        await LogAuditEventAsync(user.Id, email, AuthAuditEventType.PASSWORD_RESET,
            null, null, true, null, cancellationToken);

        LogPasswordResetWithCodeCompletedForUserUserid(user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailVerificationTokenAsync(token, cancellationToken);
        if (user == null)
        {
            return AuthResult.Failed(AuthErrorCode.INVALID_TOKEN);
        }

        if (user.EmailVerified)
        {
            return AuthResult.Failed(AuthErrorCode.EMAIL_ALREADY_VERIFIED);
        }

        var success = user.VerifyEmailWithToken(token);
        if (!success)
        {
            return AuthResult.Failed(AuthErrorCode.INVALID_TOKEN);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.EMAIL_VERIFICATION,
            null, null, true, null, cancellationToken);

        LogEmailVerifiedForUserUserid(user.Id.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> ResendVerificationEmailAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return AuthResult.Failed(AuthErrorCode.USER_NOT_FOUND);
        }

        if (user.EmailVerified)
        {
            return AuthResult.Failed(AuthErrorCode.EMAIL_ALREADY_VERIFIED);
        }

        user.GenerateEmailVerificationToken();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // TODO: Send verification email via IEmailService
        // await _emailService.SendVerificationEmailAsync(user, cancellationToken);

        LogVerificationEmailResentForUserUserid(userId.Value);

        return AuthResult.Succeeded();
    }

    /// <inheritdoc />
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Validate and rotate the refresh token
        var result = await _tokenService.RefreshTokensAsync(refreshToken, null, cancellationToken);
        if (result == null)
        {
            LogTokenRefreshFailedInvalidOrExpiredRefreshToken();
            return AuthResult.Failed(AuthErrorCode.INVALID_TOKEN);
        }

        // Get the user to include full details in the response
        var user = await _userRepository.GetByIdAsync(result.UserId, cancellationToken);
        if (user == null)
        {
            LogTokenRefreshFailedUserNotFoundForIdUserid(result.UserId.Value);
            return AuthResult.Failed(AuthErrorCode.USER_NOT_FOUND);
        }

        var authenticatedUser = new AuthenticatedUser
        {
            FamilyId = user.FamilyId,
            EmailVerified = user.EmailVerified
        };

        await LogAuditEventAsync(user.Id, user.Email, AuthAuditEventType.TOKEN_REFRESH,
            null, null, true, null, cancellationToken);

        LogTokenRefreshedForUserUserid(user.Id.Value);

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

    [LoggerMessage(LogLevel.Information, "User registered successfully: {email}")]
    partial void LogUserRegisteredSuccessfullyEmail(string email);

    [LoggerMessage(LogLevel.Warning, "Account locked due to failed login attempts: {email}")]
    partial void LogAccountLockedDueToFailedLoginAttemptsEmail(string email);

    [LoggerMessage(LogLevel.Information, "User logged in successfully: {email}")]
    partial void LogUserLoggedInSuccessfullyEmail(string email);

    [LoggerMessage(LogLevel.Information, "User logged out successfully")]
    partial void LogUserLoggedOutSuccessfully();

    [LoggerMessage(LogLevel.Information, "User logged out from all devices. Sessions revoked: {count}")]
    partial void LogUserLoggedOutFromAllDevicesSessionsRevokedCount(int count);

    [LoggerMessage(LogLevel.Information, "Password changed for user: {userId}")]
    partial void LogPasswordChangedForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Debug, "Password reset requested for non-existent email: {email}")]
    partial void LogPasswordResetRequestedForNonExistentEmailEmail(string email);

    [LoggerMessage(LogLevel.Information, "Password reset requested for user: {email}")]
    partial void LogPasswordResetRequestedForUserEmail(string email);

    [LoggerMessage(LogLevel.Information, "Password reset completed for user: {userId}")]
    partial void LogPasswordResetCompletedForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Information, "Password reset with code completed for user: {userId}")]
    partial void LogPasswordResetWithCodeCompletedForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Information, "Email verified for user: {userId}")]
    partial void LogEmailVerifiedForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Information, "Verification email resent for user: {userId}")]
    partial void LogVerificationEmailResentForUserUserid(Guid userId);

    [LoggerMessage(LogLevel.Warning, "Token refresh failed: invalid or expired refresh token")]
    partial void LogTokenRefreshFailedInvalidOrExpiredRefreshToken();

    [LoggerMessage(LogLevel.Warning, "Token refresh failed: user not found for ID {userId}")]
    partial void LogTokenRefreshFailedUserNotFoundForIdUserid(Guid userId);

    [LoggerMessage(LogLevel.Debug, "Token refreshed for user: {userId}")]
    partial void LogTokenRefreshedForUserUserid(Guid userId);
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
