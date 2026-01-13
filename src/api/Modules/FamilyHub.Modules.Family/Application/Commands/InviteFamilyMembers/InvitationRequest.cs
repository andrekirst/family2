using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;

/// <summary>
/// Represents a single invitation request within a batch invitation operation.
/// </summary>
/// <param name="Email">The email address of the person to invite.</param>
/// <param name="Role">The role to assign to the invited member (Admin, Member, or Child - not Owner).</param>
public record InvitationRequest(
    Email Email,
    FamilyRole Role
);
