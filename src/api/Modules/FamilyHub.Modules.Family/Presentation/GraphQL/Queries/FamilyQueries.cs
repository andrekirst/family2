using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for family operations.
/// Uses SharedKernel.IUserContext abstraction for authenticated user context.
/// </summary>
[ExtendObjectType("Query")]
public sealed class FamilyQueries
{
    /// <summary>
    /// Gets the current authenticated user's family.
    /// Returns null if the user is not found.
    /// Uses HotChocolate projections for automatic field selection optimization.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get the current user's family")]
    [UseProjection]
    public async Task<Domain.Aggregates.Family?> Family(
        [Service] IUserContext userContext,
        [Service] IFamilyService familyService,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get family via service
        var familyDto = await familyService.GetFamilyByUserIdAsync(userId, cancellationToken);
        if (familyDto == null)
        {
            return null;
        }

        // Reconstitute aggregate for GraphQL type resolution
        // TODO: Consider returning DTO directly with dedicated GraphQL type mapping
        return Domain.Aggregates.Family.Reconstitute(
            familyDto.Id,
            familyDto.Name,
            familyDto.OwnerId,
            familyDto.CreatedAt,
            familyDto.UpdatedAt);
    }
}
