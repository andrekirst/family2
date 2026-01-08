using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for family operations.
/// PHASE 4: Moved to Family module using SharedKernel.IUserContext abstraction.
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

        // Reconstitute aggregate for GraphQL (Phase 3 - temporary)
        // TODO Phase 5+: Return DTO directly or use proper GraphQL type mapping
        return Domain.Aggregates.Family.Reconstitute(
            familyDto.Id,
            familyDto.Name,
            familyDto.OwnerId,
            familyDto.CreatedAt,
            familyDto.UpdatedAt);
    }
}
