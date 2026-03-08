using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.PreviewRuleMatch;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<RuleMatchPreviewDto?> PreviewRuleMatch(
        Guid fileId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new PreviewRuleMatchQuery(
            FileId.From(fileId));
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
