using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

/// <summary>
/// Command to accept a family invitation using the plaintext token.
/// </summary>
public sealed record AcceptInvitationCommand(
    string Token,
    UserId AcceptingUserId
) : ICommand<AcceptInvitationResult>;
