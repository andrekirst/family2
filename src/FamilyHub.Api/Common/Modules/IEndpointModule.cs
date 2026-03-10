namespace FamilyHub.Api.Common.Modules;

/// <summary>
/// Interface for modules that register REST/minimal API endpoints.
/// Modules implementing this interface will have their endpoints mapped
/// during application startup via MapEndpoints().
/// </summary>
public interface IEndpointModule
{
    void MapEndpoints(WebApplication app);
}
