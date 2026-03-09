using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.DownloadFile;

public sealed class DownloadFileQueryHandler(
    IFileManagementStorageService storageService)
    : IQueryHandler<DownloadFileQuery, Result<DownloadFileResult>>
{
    public async ValueTask<Result<DownloadFileResult>> Handle(
        DownloadFileQuery query,
        CancellationToken cancellationToken)
    {
        var downloadResult = await storageService.GetFileAsync(query.StorageKey, cancellationToken);
        if (downloadResult is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        return new DownloadFileResult(
            downloadResult.Data,
            downloadResult.MimeType,
            downloadResult.Size);
    }
}
