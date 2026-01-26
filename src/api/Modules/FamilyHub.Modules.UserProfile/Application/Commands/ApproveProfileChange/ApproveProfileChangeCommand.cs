using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
using DomainResult = FamilyHub.SharedKernel.Domain.Result<FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange.ApproveProfileChangeResult>;

namespace FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange;

/// <summary>
/// Command to approve a pending profile change request.
/// Only users with Owner or Admin role can approve changes.
/// </summary>
/// <param name="RequestId">The ID of the change request to approve.</param>
public sealed record ApproveProfileChangeCommand(
    ChangeRequestId RequestId
) : ICommand<DomainResult>,
    IRequireAuthentication,
    IRequireOwnerOrAdminRole;
