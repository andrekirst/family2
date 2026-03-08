using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Infrastructure.FamilyScope;

/// <summary>
/// Marker interface for commands/queries that target a specific family.
/// The FamilyMembershipBehavior extracts the FamilyId and verifies the user is a member of that family.
/// </summary>
public interface IFamilyScoped
{
    FamilyId FamilyId { get; }
}
