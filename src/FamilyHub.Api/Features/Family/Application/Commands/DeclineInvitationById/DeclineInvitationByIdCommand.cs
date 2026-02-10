using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitationById;

/// <summary>
/// Command to decline a family invitation by its ID (from the dashboard).
/// Requires email verification to ensure only the intended recipient can decline.
/// </summary>
public sealed record DeclineInvitationByIdCommand(
    InvitationId InvitationId,
    UserId DeclininingUserId
) : ICommand<bool>;
