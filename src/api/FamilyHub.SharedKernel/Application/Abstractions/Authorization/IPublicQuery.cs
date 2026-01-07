namespace FamilyHub.SharedKernel.Application.Abstractions.Authorization;

/// <summary>
/// Marker interface indicating that a query does not require authentication.
/// UserContextEnrichmentBehavior and AuthorizationBehavior will skip processing for these requests.
/// </summary>
public interface IPublicQuery
{
}
