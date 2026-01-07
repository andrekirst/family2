namespace FamilyHub.SharedKernel.Application.Abstractions.Authorization;

/// <summary>
/// Marker interface indicating that a request requires an authenticated user with Owner or Admin role.
/// AuthorizationBehavior will enforce the "RequireOwnerOrAdmin" authorization policy.
/// </summary>
public interface IRequireOwnerOrAdminRole : IRequireAuthentication
{
}
