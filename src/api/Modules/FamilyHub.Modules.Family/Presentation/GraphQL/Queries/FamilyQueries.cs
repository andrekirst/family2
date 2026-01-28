using FamilyHub.Modules.Family.Application.Queries.GetUserFamily;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Queries;

/// <summary>
/// GraphQL queries for family operations.
/// Dispatches to MediatR handlers to ensure UserContextEnrichmentBehavior runs.
/// </summary>
[ExtendObjectType("Query")]
public sealed class FamilyQueries
{
    /// <summary>
    /// Gets the current authenticated user's family.
    /// Returns null if the user has no family.
    /// Dispatches to MediatR to trigger UserContextEnrichmentBehavior.
    /// </summary>
    [Authorize]
    [GraphQLDescription("Get the current user's family")]
    [UseProjection]
    public async Task<Domain.Aggregates.Family?> Family(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Dispatch to MediatR - this triggers UserContextEnrichmentBehavior
        // which populates IUserContext before the handler runs
        var result = await mediator.Send<GetUserFamilyResult?>(new GetUserFamilyQuery(), cancellationToken);

        if (result == null)
        {
            return null;
        }

        // Reconstitute aggregate for GraphQL type resolution
        return Domain.Aggregates.Family.Reconstitute(
            result.FamilyId,
            FamilyName.From(result.Name),
            result.OwnerId,
            result.CreatedAt,
            result.UpdatedAt);
    }
}
