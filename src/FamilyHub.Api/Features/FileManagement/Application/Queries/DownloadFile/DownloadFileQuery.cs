using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.DownloadFile;

public sealed record DownloadFileQuery(
    string StorageKey
) : IReadOnlyQuery<Result<DownloadFileResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
