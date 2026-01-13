using System.Linq.Expressions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for finding an invitation by its ID.
/// Replaces: GetByIdAsync(InvitationId)
/// </summary>
/// <param name="id">The invitation ID to search for.</param>
public sealed class InvitationByIdSpecification(InvitationId id) : Specification<FamilyMemberInvitation>
{
    /// <inheritdoc/>
    public override Expression<Func<FamilyMemberInvitation, bool>> ToExpression()
        => invitation => invitation.Id == id;
}
