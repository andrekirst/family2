namespace FamilyHub.Modules.Auth.Domain.Exceptions;

/// <summary>
/// Exception thrown when a refresh token is invalid, expired, or revoked.
/// </summary>
public sealed class InvalidRefreshTokenException() : Exception("Invalid or expired refresh token.");
