namespace FamilyHub.SharedKernel.Application.Abstractions.Authorization;

/// <summary>
/// Base marker interface indicating that a request requires an authenticated user.
/// UserContextEnrichmentBehavior will load the User aggregate for requests implementing this interface.
/// </summary>
public interface IRequireAuthentication
{
}
