using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

/// <summary>
/// Command to revoke a pending family invitation (by an admin/owner).
/// </summary>
public sealed record RevokeInvitationCommand(
    InvitationId InvitationId,
    UserId RevokedBy
) : ICommand<bool>;
