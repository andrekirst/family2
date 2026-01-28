using FamilyHub.Modules.Family.Application.Queries.GetUserFamily;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;

namespace FamilyHub.Modules.Family.Presentation.GraphQL.Namespaces;

/// <summary>
/// Extends FamilyQueries with family-related queries.
/// Access pattern: query { family { current { ... }, members { ... } } }
/// </summary>
/// <remarks>
/// <para>
/// This extension adds family queries to the family namespace, providing
/// a domain-centric query structure for family data.
/// </para>
/// </remarks>
[ExtendObjectType(typeof(FamilyQueries))]
public sealed class FamilyQueriesExtensions
{
    /// <summary>
    /// Gets the current authenticated user's family.
    /// Returns null if the user has no family.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's family or null.</returns>
    [Authorize]
    [GraphQLDescription("Get the current user's family.")]
    [UseProjection]
    public async Task<Domain.Aggregates.Family?> Current(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send<GetUserFamilyResult?>(new GetUserFamilyQuery(), cancellationToken);

        if (result == null)
        {
            return null;
        }

        return Domain.Aggregates.Family.Reconstitute(
            result.FamilyId,
            FamilyName.From(result.Name),
            result.OwnerId,
            result.CreatedAt,
            result.UpdatedAt);
    }
}
