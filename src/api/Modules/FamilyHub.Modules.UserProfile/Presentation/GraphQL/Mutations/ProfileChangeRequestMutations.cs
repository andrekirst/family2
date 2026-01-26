using FamilyHub.Infrastructure.GraphQL.Interceptors;
using FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange;
using FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Inputs;
using FamilyHub.Modules.UserProfile.Presentation.GraphQL.Types;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Modules.UserProfile.Presentation.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for profile change request operations.
/// Authorization requires Owner or Admin role for approve/reject operations.
/// </summary>
[ExtendObjectType("Mutation")]
public sealed class ProfileChangeRequestMutations : IRequireAuthentication, IRequireOwnerOrAdminRole
{
    /// <summary>
    /// Approves a pending profile change request.
    /// Applies the change to the profile and marks the request as approved.
    /// </summary>
    [Authorize]
    [DefaultMutationErrors]
    [UseMutationConvention]
    [GraphQLDescription("Approve a pending profile change request (Owner/Admin only)")]
    public async Task<ApproveProfileChangePayload> ApproveProfileChange(
        ApproveProfileChangeInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ApproveProfileChangeCommand(
            ChangeRequestId.From(input.RequestId)
        );

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<ApproveProfileChangeResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(result.Error)
                    .SetCode("APPROVE_CHANGE_FAILED")
                    .Build());
        }

        return new ApproveProfileChangePayload
        {
            RequestId = result.Value.RequestId.Value,
            ProfileId = result.Value.ProfileId.Value,
            FieldName = result.Value.FieldName,
            NewValue = result.Value.NewValue,
            ApprovedAt = result.Value.ApprovedAt
        };
    }

    /// <summary>
    /// Rejects a pending profile change request.
    /// Marks the request as rejected with the provided reason.
    /// </summary>
    [Authorize]
    [DefaultMutationErrors]
    [UseMutationConvention]
    [GraphQLDescription("Reject a pending profile change request (Owner/Admin only)")]
    public async Task<RejectProfileChangePayload> RejectProfileChange(
        RejectProfileChangeInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RejectProfileChangeCommand(
            ChangeRequestId.From(input.RequestId),
            input.Reason
        );

        var result = await mediator.Send<FamilyHub.SharedKernel.Domain.Result<RejectProfileChangeResult>>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(result.Error)
                    .SetCode("REJECT_CHANGE_FAILED")
                    .Build());
        }

        return new RejectProfileChangePayload
        {
            RequestId = result.Value.RequestId.Value,
            ProfileId = result.Value.ProfileId.Value,
            FieldName = result.Value.FieldName,
            Reason = result.Value.Reason,
            RejectedAt = result.Value.RejectedAt
        };
    }
}
