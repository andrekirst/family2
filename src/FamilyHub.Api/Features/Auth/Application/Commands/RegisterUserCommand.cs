using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Application.Commands;

/// <summary>
/// Command to register a new user or update existing user from OAuth provider.
/// </summary>
public sealed record RegisterUserCommand(
    Email Email,
    UserName Name,
    ExternalUserId ExternalUserId,
    bool EmailVerified,
    string? Username = null
) : ICommand<RegisterUserResult>;

/// <summary>
/// Result of user registration command.
/// </summary>
public sealed record RegisterUserResult(
    UserId UserId,
    bool IsNewUser
);
