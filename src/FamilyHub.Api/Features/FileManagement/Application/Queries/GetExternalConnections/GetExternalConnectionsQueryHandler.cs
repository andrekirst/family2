using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetExternalConnections;

public sealed class GetExternalConnectionsQueryHandler(
    IExternalConnectionRepository connectionRepository)
    : IQueryHandler<GetExternalConnectionsQuery, List<ExternalConnectionDto>>
{
    public async ValueTask<List<ExternalConnectionDto>> Handle(
        GetExternalConnectionsQuery query,
        CancellationToken cancellationToken)
    {
        var connections = await connectionRepository.GetByFamilyIdAsync(
            query.FamilyId, cancellationToken);

        return connections.Select(FileManagementMapper.ToDto).ToList();
    }
}
