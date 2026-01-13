using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Auth.Application.Queries.GetPendingInvitations;

/// <summary>
/// Query to retrieve all pending invitations for the authenticated user's family.
/// Requires Owner or Admin role.
/// FamilyId is automatically extracted from IUserContext by the handler.
/// </summary>
public sealed record GetPendingInvitationsQuery : IQuery<GetPendingInvitationsResult>,
      IRequireFamilyContext,
      IRequireOwnerOrAdminRole;
