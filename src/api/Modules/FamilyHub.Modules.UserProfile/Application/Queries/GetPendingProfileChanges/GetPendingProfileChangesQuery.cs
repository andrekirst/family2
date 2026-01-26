using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetPendingProfileChanges;

/// <summary>
/// Query to get all pending profile change requests for the current user's family.
/// Only users with Owner or Admin role can view pending changes.
/// </summary>
public sealed record GetPendingProfileChangesQuery
    : IQuery<GetPendingProfileChangesResult>,
      IRequireAuthentication,
      IRequireOwnerOrAdminRole;
