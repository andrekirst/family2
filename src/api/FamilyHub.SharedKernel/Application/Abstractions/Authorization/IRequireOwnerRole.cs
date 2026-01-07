namespace FamilyHub.SharedKernel.Application.Abstractions.Authorization;

/// <summary>
/// Marker interface indicating that a request requires an authenticated user with Owner role.
/// AuthorizationBehavior will enforce the "RequireOwner" authorization policy.
/// </summary>
public interface IRequireOwnerRole : IRequireAuthentication
{
}
