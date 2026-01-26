using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.UserProfile.Application.Queries.GetMyPendingChanges;

/// <summary>
/// Query to get the current user's pending profile change requests.
/// Available to any authenticated user (primarily used by child users).
/// </summary>
public sealed record GetMyPendingChangesQuery
    : IQuery<GetMyPendingChangesResult>,
      IRequireAuthentication;
