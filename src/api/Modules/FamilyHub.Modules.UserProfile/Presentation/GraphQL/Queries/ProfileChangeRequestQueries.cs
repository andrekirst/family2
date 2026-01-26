using FamilyHub.Modules.UserProfile.Application.Queries.GetMyPendingChanges;
using FamilyHub.Modules.UserProfile.Application.Queries.GetPendingProfileChanges;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for profile change request operations.
/// </summary>
[ExtendObjectType("Query")]
public sealed class ProfileChangeRequestQueries
{
    /// <summary>
    /// Gets all pending profile change requests for the current user's family.
    /// Only available to users with Owner or Admin role.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get pending profile changes for approval (Owner/Admin only)")]
    public async Task<PendingProfileChangesDto> PendingProfileChanges(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPendingProfileChangesQuery();
        var result = await mediator.Send<GetPendingProfileChangesResult>(query, cancellationToken);

        return new PendingProfileChangesDto
        {
            ChangeRequests = result.ChangeRequests.Select(r => new ProfileChangeRequestDto
            {
                Id = r.RequestId.Value,
                ProfileId = r.ProfileId.Value,
                RequestedBy = r.RequestedBy.Value,
                RequestedByDisplayName = r.RequestedByDisplayName,
                FieldName = r.FieldName,
                OldValue = r.OldValue,
                NewValue = r.NewValue,
                Status = "pending",
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    /// <summary>
    /// Gets the current user's pending and recently rejected profile change requests.
    /// Primarily used by child users to see their pending changes.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get my pending profile changes")]
    public async Task<MyPendingChangesDto> MyPendingChanges(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetMyPendingChangesQuery();
        var result = await mediator.Send<GetMyPendingChangesResult>(query, cancellationToken);

        return new MyPendingChangesDto
        {
            PendingChanges = result.PendingChanges.Select(p => new MyPendingChangeDto
            {
                Id = p.RequestId.Value,
                FieldName = p.FieldName,
                OldValue = p.OldValue,
                NewValue = p.NewValue,
                CreatedAt = p.CreatedAt
            }).ToList(),
            RecentlyRejected = result.RecentlyRejected.Select(r => new MyRejectedChangeDto
            {
                Id = r.RequestId.Value,
                FieldName = r.FieldName,
                RequestedValue = r.RequestedValue,
                RejectionReason = r.RejectionReason,
                RejectedAt = r.RejectedAt
            }).ToList()
        };
    }
}
