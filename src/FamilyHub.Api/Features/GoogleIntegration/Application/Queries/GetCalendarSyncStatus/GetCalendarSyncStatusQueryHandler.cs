using FamilyHub.Common.Application;
using FamilyHub.Api.Features.GoogleIntegration.Application.Mappers;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration.Application.Queries.GetCalendarSyncStatus;

public sealed class GetCalendarSyncStatusQueryHandler(
    IGoogleAccountLinkRepository linkRepository)
    : IQueryHandler<GetCalendarSyncStatusQuery, GoogleCalendarSyncStatusDto>
{
    public async ValueTask<GoogleCalendarSyncStatusDto> Handle(
        GetCalendarSyncStatusQuery query,
        CancellationToken cancellationToken)
    {
        var link = await linkRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        return GoogleIntegrationMapper.ToCalendarSyncStatusDto(link);
    }
}
