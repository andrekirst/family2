using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetAlbums;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<AlbumDto>> GetAlbums(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetAlbumsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
