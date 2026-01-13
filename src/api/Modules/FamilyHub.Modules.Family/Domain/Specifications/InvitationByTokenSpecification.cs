using System.Linq.Expressions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.Family.Domain.Specifications;

/// <summary>
/// Specification for finding an invitation by its token.
/// Replaces: GetByTokenAsync(InvitationToken)
/// </summary>
/// <param name="token">The invitation token to search for.</param>
public sealed class InvitationByTokenSpecification(InvitationToken token) : Specification<FamilyMemberInvitation>
{
    /// <inheritdoc/>
    public override Expression<Func<FamilyMemberInvitation, bool>> ToExpression()
        => invitation => invitation.Token == token;
}
