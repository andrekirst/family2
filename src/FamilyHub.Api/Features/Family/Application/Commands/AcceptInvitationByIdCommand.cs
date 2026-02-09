using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

/// <summary>
/// Command to accept a family invitation by its ID (from the dashboard).
/// Unlike AcceptInvitationCommand which uses a token, this uses the invitation ID directly.
/// Requires email verification to ensure only the intended recipient can accept.
/// </summary>
public sealed record AcceptInvitationByIdCommand(
    InvitationId InvitationId,
    UserId AcceptingUserId
) : ICommand<AcceptInvitationResult>;
