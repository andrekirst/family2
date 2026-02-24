using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetProcessingLog;

public sealed class GetProcessingLogQueryHandler(
    IProcessingLogRepository logRepository)
    : IQueryHandler<GetProcessingLogQuery, List<ProcessingLogEntryDto>>
{
    public async ValueTask<List<ProcessingLogEntryDto>> Handle(
        GetProcessingLogQuery query,
        CancellationToken cancellationToken)
    {
        var entries = await logRepository.GetByFamilyIdAsync(
            query.FamilyId, query.Skip, query.Take, cancellationToken);

        return entries.Select(FileManagementMapper.ToDto).ToList();
    }
}
