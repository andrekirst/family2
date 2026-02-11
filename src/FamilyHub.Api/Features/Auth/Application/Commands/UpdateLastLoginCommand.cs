using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Auth.Application.Commands;

/// <summary>
/// Command to update a user's last login timestamp.
/// </summary>
public sealed record UpdateLastLoginCommand(
    ExternalUserId ExternalUserId,
    DateTime LoginTime
) : ICommand<bool>;
