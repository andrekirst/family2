using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinkAccessLog;

public sealed record GetShareLinkAccessLogQuery(
    ShareLinkId ShareLinkId
) : IReadOnlyQuery<Result<List<ShareLinkAccessLogDto>>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
