using System.Security.Claims;
using FamilyHub.Api.Application.Queries;
using HotChocolate.Authorization;
using MediatR;

namespace FamilyHub.Api.GraphQL;

public class Queries
{
    [Authorize]
    public async Task<MeResult?> Me(
        ClaimsPrincipal claimsPrincipal,
        [Service] IMediator mediator,
        CancellationToken ct)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? claimsPrincipal.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return null;

        var query = new MeQuery(userId);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess ? result.Value : null;
    }
}
