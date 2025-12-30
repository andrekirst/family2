using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Auth.Presentation.GraphQL.Types;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Adapters;

/// <summary>
/// Adapter for mapping domain results to FamilyType (GraphQL output).
/// Centralizes mapping logic for family-related GraphQL responses.
/// </summary>
public static class FamilyOutputAdapter
{
    /// <summary>
    /// Maps CreateFamilyResult to FamilyType for GraphQL response.
    /// </summary>
    /// <param name="result">Command result from application layer</param>
    /// <returns>FamilyType for GraphQL response</returns>
    public static FamilyType ToGraphQLType(CreateFamilyResult result)
    {
        return new FamilyType
        {
            Id = result.FamilyId.Value,
            Name = result.Name.Value,
            OwnerId = result.OwnerId.Value,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.CreatedAt // Same as CreatedAt for newly created families
        };
    }
}
