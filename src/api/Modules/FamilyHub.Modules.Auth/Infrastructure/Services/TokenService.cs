using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Token service for JWT access tokens and refresh token management.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the TokenService.
    /// </summary>
    /// <param name="jwtSettings">JWT configuration settings.</param>
    /// <param name="refreshTokenRepository">Repository for refresh token persistence.</param>
    /// <param name="userRepository">Repository for user lookups.</param>
    /// <param name="timeProvider">Time provider for consistent timestamps.</param>
    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        TimeProvider timeProvider)
    {
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<TokenPair> GenerateTokenPairAsync(
        User user,
        string? deviceInfo = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        // Generate access token
        var accessToken = GenerateAccessToken(user, now);
        var accessTokenExpires = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        // Generate refresh token
        var (refreshToken, refreshTokenHash) = GenerateRefreshToken();
        var refreshTokenLifetime = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays);
        var refreshTokenExpires = now.Add(refreshTokenLifetime);

        // Store refresh token in database
        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            refreshTokenHash,
            refreshTokenLifetime,
            deviceInfo,
            ipAddress);

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpires,
            RefreshTokenExpiresAt = refreshTokenExpires
        };
    }

    /// <inheritdoc />
    public async Task<RefreshResult?> RefreshTokensAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashRefreshToken(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken == null || existingToken.IsRevoked || existingToken.IsExpired)
        {
            return null;
        }

        // Get the user
        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user == null || user.IsLockedOut)
        {
            return null;
        }

        // Rotate refresh token (revoke old, create new)
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        // Generate new tokens
        var accessToken = GenerateAccessToken(user, now);
        var accessTokenExpires = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var (newRefreshToken, newRefreshTokenHash) = GenerateRefreshToken();
        var refreshTokenLifetime = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays);
        var refreshTokenExpires = now.Add(refreshTokenLifetime);

        // Create new refresh token entity
        var newRefreshTokenEntity = Domain.RefreshToken.Create(
            user.Id,
            newRefreshTokenHash,
            refreshTokenLifetime,
            existingToken.DeviceInfo, // Preserve device info
            ipAddress ?? existingToken.IpAddress);

        // Revoke old token and link to new one
        existingToken.Revoke(newRefreshTokenEntity.Id);

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        return new RefreshResult
        {
            UserId = user.Id,
            Tokens = new TokenPair
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = accessTokenExpires,
                RefreshTokenExpiresAt = refreshTokenExpires
            }
        };
    }

    /// <inheritdoc />
    public async Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashRefreshToken(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken == null)
        {
            return false;
        }

        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<int> RevokeAllUserTokensAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);
        var count = 0;

        foreach (var token in activeTokens)
        {
            token.Revoke();
            await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
            count++;
        }

        return count;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ActiveSession>> GetActiveSessionsAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);

        return activeTokens
            .Select(t => new ActiveSession
            {
                SessionId = t.Id.Value,
                DeviceInfo = t.DeviceInfo,
                IpAddress = t.IpAddress,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                IsCurrent = false // Caller should determine based on current token
            })
            .ToList();
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            return tokenHandler.ValidateToken(accessToken, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    private string GenerateAccessToken(User user, DateTime now)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("family_id", user.FamilyId.Value.ToString()),
            new("role", user.Role.Value),
            new("email_verified", user.EmailVerified.ToString().ToLowerInvariant())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = signingCredentials,
            IssuedAt = now,
            NotBefore = now
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static (string Token, string Hash) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var token = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var hash = HashRefreshToken(token);
        return (token, hash);
    }

    private static string HashRefreshToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}
