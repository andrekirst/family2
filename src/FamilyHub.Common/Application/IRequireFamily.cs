using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for commands/queries that require an authenticated user
/// who is a member of a family. Extends <see cref="IRequireUser"/> with FamilyId.
/// The UserResolutionBehavior validates family membership and populates both
/// UserId and FamilyId via the <c>with</c> expression.
/// </summary>
public interface IRequireFamily : IRequireUser
{
    FamilyId FamilyId { get; init; }
}
