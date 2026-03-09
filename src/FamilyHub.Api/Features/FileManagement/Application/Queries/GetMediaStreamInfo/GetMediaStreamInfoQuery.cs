using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetMediaStreamInfo;

public sealed record GetMediaStreamInfoQuery(
    FileId FileId
) : IReadOnlyQuery<Result<MediaStreamInfoDto>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
