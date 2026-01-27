using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Modules.Auth.Application.Commands.Logout;

/// <summary>
/// Command to log out the current user.
/// Optionally revokes a specific refresh token, or all tokens if not specified.
/// </summary>
public sealed record LogoutCommand(
    string? RefreshToken = null,
    bool LogoutAllDevices = false
) : ICommand<FamilyHub.SharedKernel.Domain.Result<LogoutResult>>,
    IRequireAuthentication;

/// <summary>
/// Result of logout operation.
/// </summary>
public sealed record LogoutResult
{
    /// <summary>
    /// Indicates the logout was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Number of sessions (refresh tokens) revoked.
    /// </summary>
    public int RevokedSessionCount { get; init; }
}
