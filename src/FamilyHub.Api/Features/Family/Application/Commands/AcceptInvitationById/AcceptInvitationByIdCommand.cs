using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Application.Commands.Shared;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

/// <summary>
/// Command to accept a family invitation by its ID (from the dashboard).
/// Unlike AcceptInvitationCommand which uses a token, this uses the invitation ID directly.
/// Requires email verification to ensure only the intended recipient can accept.
/// </summary>
public sealed record AcceptInvitationByIdCommand(
    InvitationId InvitationId,
    UserId AcceptingUserId
) : ICommand<AcceptInvitationResult>;
