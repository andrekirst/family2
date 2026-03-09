using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.StreamFile;

public sealed record StreamFileResult(
    Stream Data,
    string MimeType,
    long? ContentLength,
    long? RangeStart,
    long? RangeEnd,
    long? TotalSize,
    bool IsPartialContent
) : IStreamableResult;
