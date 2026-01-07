namespace FamilyHub.SharedKernel.Application.Abstractions.Authorization;

/// <summary>
/// Marker interface indicating that a request requires an authenticated user with Admin role.
/// AuthorizationBehavior will enforce the "RequireAdmin" authorization policy.
/// </summary>
public interface IRequireAdminRole : IRequireAuthentication
{
}
