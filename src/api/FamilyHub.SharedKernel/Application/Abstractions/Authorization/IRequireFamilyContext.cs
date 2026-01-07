namespace FamilyHub.SharedKernel.Application.Abstractions.Authorization;

/// <summary>
/// Marker interface indicating that a request requires an authenticated user with family context.
/// AuthorizationBehavior will verify that the user belongs to a family.
/// </summary>
public interface IRequireFamilyContext : IRequireAuthentication
{
}
