namespace FamilyHub.Api.Common.Infrastructure.FamilyScope;

/// <summary>
/// Marker interface for commands/queries that do not require family membership.
/// Commands implementing this interface bypass the FamilyMembershipBehavior check.
/// </summary>
public interface IIgnoreFamilyMembership;
