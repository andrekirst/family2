namespace FamilyHub.Common.Application;

/// <summary>
/// Marker interface for commands/queries that do not require user resolution.
/// Used for token-based operations (e.g., AcceptInvitation), public endpoints
/// (e.g., AccessShareLink), or special flows where the user is not yet in the database
/// (e.g., RegisterUser). The UserResolutionBehavior skips these entirely.
/// </summary>
public interface IAnonymousOperation;
