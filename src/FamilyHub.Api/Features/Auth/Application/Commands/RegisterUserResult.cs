using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Application.Commands;

/// <summary>
/// Result of user registration command.
/// </summary>
public sealed record RegisterUserResult(
    UserId UserId,
    bool IsNewUser
);