using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

/// <summary>
/// Command to send a family invitation to an email address.
/// </summary>
public sealed record SendInvitationCommand(
    FamilyId FamilyId,
    UserId InvitedBy,
    Email InviteeEmail,
    FamilyRole Role
) : ICommand<SendInvitationResult>;
