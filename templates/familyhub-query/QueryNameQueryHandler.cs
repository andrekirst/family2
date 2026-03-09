using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.ModuleName.Application.Queries.QueryName;

public sealed class QueryNameQueryHandler
    : IQueryHandler<QueryNameQuery, QueryNameResult>
{
    public async ValueTask<QueryNameResult> Handle(
        QueryNameQuery query,
        CancellationToken cancellationToken)
    {
        // TODO: Implement query logic

        return new QueryNameResult(true);
    }
}
