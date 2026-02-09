using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands;

/// <summary>
/// Command to revoke a pending family invitation (by an admin/owner).
/// </summary>
public sealed record RevokeInvitationCommand(
    InvitationId InvitationId,
    UserId RevokedBy
) : ICommand<bool>;
