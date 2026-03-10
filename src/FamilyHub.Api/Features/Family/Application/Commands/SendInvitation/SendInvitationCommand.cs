using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

/// <summary>
/// Command to send a family invitation to an email address.
/// </summary>
public sealed record SendInvitationCommand(
    Email InviteeEmail,
    FamilyRole Role
) : ICommand<SendInvitationResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
