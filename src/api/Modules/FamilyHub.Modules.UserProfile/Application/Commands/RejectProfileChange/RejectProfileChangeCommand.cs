using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
using DomainResult = FamilyHub.SharedKernel.Domain.Result<FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange.RejectProfileChangeResult>;

namespace FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange;

/// <summary>
/// Command to reject a pending profile change request.
/// Only users with Owner or Admin role can reject changes.
/// </summary>
/// <param name="RequestId">The ID of the change request to reject.</param>
/// <param name="Reason">The reason for rejection (minimum 10 characters).</param>
public sealed record RejectProfileChangeCommand(
    ChangeRequestId RequestId,
    string Reason
) : ICommand<DomainResult>,
    IRequireAuthentication,
    IRequireOwnerOrAdminRole;
