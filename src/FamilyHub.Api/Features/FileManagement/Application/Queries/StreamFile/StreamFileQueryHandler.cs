using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.StreamFile;

public sealed class StreamFileQueryHandler(
    IFileManagementStorageService storageService)
    : IQueryHandler<StreamFileQuery, Result<StreamFileResult>>
{
    public async ValueTask<Result<StreamFileResult>> Handle(
        StreamFileQuery query,
        CancellationToken cancellationToken)
    {
        if (query.RangeFrom is null)
        {
            return await HandleFullDownload(query.StorageKey, cancellationToken);
        }

        return await HandleRangeDownload(
            query.StorageKey,
            query.RangeFrom.Value,
            query.RangeTo ?? long.MaxValue,
            cancellationToken);
    }

    private async ValueTask<Result<StreamFileResult>> HandleFullDownload(
        string storageKey,
        CancellationToken cancellationToken)
    {
        var downloadResult = await storageService.GetFileAsync(storageKey, cancellationToken);
        if (downloadResult is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        return new StreamFileResult(
            Data: downloadResult.Data,
            MimeType: downloadResult.MimeType,
            ContentLength: downloadResult.Size,
            RangeStart: null,
            RangeEnd: null,
            TotalSize: downloadResult.Size,
            IsPartialContent: false);
    }

    private async ValueTask<Result<StreamFileResult>> HandleRangeDownload(
        string storageKey,
        long rangeFrom,
        long rangeTo,
        CancellationToken cancellationToken)
    {
        var rangeResult = await storageService.GetFileRangeAsync(
            storageKey, rangeFrom, rangeTo, cancellationToken);
        if (rangeResult is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        var contentLength = rangeResult.RangeEnd - rangeResult.RangeStart + 1;

        return new StreamFileResult(
            Data: rangeResult.Data,
            MimeType: "application/octet-stream",
            ContentLength: contentLength,
            RangeStart: rangeResult.RangeStart,
            RangeEnd: rangeResult.RangeEnd,
            TotalSize: rangeResult.TotalSize,
            IsPartialContent: true);
    }
}
