using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Application.Commands.UpdateLastLogin;

/// <summary>
/// Command to update a user's last login timestamp.
/// </summary>
public sealed record UpdateLastLoginCommand(
    ExternalUserId ExternalUserId,
    DateTime LoginTime
) : ICommand<bool>;
