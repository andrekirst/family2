using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFile;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<StoredFileDto?> GetFile(
        Guid fileId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetFileQuery(FileId.From(fileId));
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
