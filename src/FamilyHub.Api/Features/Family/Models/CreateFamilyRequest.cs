namespace FamilyHub.Api.Features.Family.Models;

/// <summary>
/// Request model for creating a new family
/// </summary>
public class CreateFamilyRequest
{
    public required string Name { get; set; }
}
