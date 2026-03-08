using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for commands/queries that require an authenticated user.
/// The UserResolutionBehavior resolves the current user from HttpContext and
/// populates UserId via the <c>with</c> expression before the handler executes.
/// </summary>
public interface IRequireUser
{
    UserId UserId { get; init; }
}
