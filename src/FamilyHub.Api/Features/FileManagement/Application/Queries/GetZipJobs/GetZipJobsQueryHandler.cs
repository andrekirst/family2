using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetZipJobs;

public sealed class GetZipJobsQueryHandler(
    IZipJobRepository zipJobRepository)
    : IQueryHandler<GetZipJobsQuery, List<ZipJobDto>>
{
    public async ValueTask<List<ZipJobDto>> Handle(
        GetZipJobsQuery query,
        CancellationToken cancellationToken)
    {
        var jobs = await zipJobRepository.GetByFamilyIdAsync(
            query.FamilyId, cancellationToken);

        return jobs.Select(FileManagementMapper.ToDto).ToList();
    }
}
