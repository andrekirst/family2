using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

/// <summary>
/// Command to send a family invitation to an email address.
/// </summary>
public sealed record SendInvitationCommand(
    FamilyId FamilyId,
    UserId InvitedBy,
    Email InviteeEmail,
    FamilyRole Role
) : ICommand<SendInvitationResult>;
