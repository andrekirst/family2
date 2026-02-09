using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.Shared;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

/// <summary>
/// Command to accept a family invitation using the plaintext token.
/// </summary>
public sealed record AcceptInvitationCommand(
    string Token,
    UserId AcceptingUserId
) : ICommand<AcceptInvitationResult>;
