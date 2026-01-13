using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;

/// <summary>
/// Command to invite multiple family members by email in a batch operation.
/// Supports partial success - valid invitations succeed, invalid ones return errors.
/// Requires Owner or Admin role.
/// </summary>
/// <param name="FamilyId">The ID of the family to invite members to.</param>
/// <param name="Invitations">The list of invitation requests (max 20).</param>
/// <param name="Message">Optional personal message for all invitations.</param>
public record InviteFamilyMembersCommand(
    FamilyId FamilyId,
    IReadOnlyList<InvitationRequest> Invitations,
    string? Message = null
) : ICommand<InviteFamilyMembersResult>,
    IRequireFamilyContext,
    IRequireOwnerOrAdminRole;
