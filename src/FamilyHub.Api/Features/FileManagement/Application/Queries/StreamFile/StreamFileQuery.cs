using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.StreamFile;

public sealed record StreamFileQuery(
    string StorageKey,
    long? RangeFrom,
    long? RangeTo
) : IReadOnlyQuery<Result<StreamFileResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
