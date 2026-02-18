using FamilyHub.Api.Features.FileManagement.Application.Services;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.GenerateThumbnails;

public sealed class GenerateThumbnailsCommandHandler(
    IStoredFileRepository fileRepository,
    IFileThumbnailRepository thumbnailRepository,
    IThumbnailGenerationService thumbnailService,
    IStorageProvider storageProvider)
    : ICommandHandler<GenerateThumbnailsCommand, GenerateThumbnailsResult>
{
    private static readonly (int Width, int Height)[] ThumbnailSizes =
    [
        (200, 200),
        (800, 800)
    ];

    public async ValueTask<GenerateThumbnailsResult> Handle(
        GenerateThumbnailsCommand command,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        if (file.FamilyId != command.FamilyId)
            throw new DomainException("File not found in this family", DomainErrorCodes.FileNotFound);

        if (!thumbnailService.CanGenerateThumbnail(file.MimeType.Value))
            return new GenerateThumbnailsResult(true, 0);

        await using var sourceStream = await storageProvider.DownloadAsync(file.StorageKey.Value, cancellationToken);
        if (sourceStream is null)
            return new GenerateThumbnailsResult(false, 0);

        using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream, cancellationToken);
        var sourceData = memoryStream.ToArray();

        if (sourceData.Length == 0)
            return new GenerateThumbnailsResult(false, 0);

        var generated = 0;
        foreach (var (width, height) in ThumbnailSizes)
        {
            var existing = await thumbnailRepository.GetByFileIdAndSizeAsync(
                command.FileId, width, height, cancellationToken);
            if (existing is not null)
                continue;

            var thumbnailData = await thumbnailService.GenerateThumbnailAsync(
                sourceData, file.MimeType.Value, width, height, cancellationToken);

            var storageKey = StorageKey.From($"thumbnails/{command.FileId.Value}/{width}x{height}.webp");
            using var thumbnailStream = new MemoryStream(thumbnailData);
            await storageProvider.UploadAsync(thumbnailStream, "image/webp", cancellationToken);

            var thumbnail = FileThumbnail.Create(command.FileId, width, height, storageKey);
            await thumbnailRepository.AddAsync(thumbnail, cancellationToken);
            generated++;
        }

        return new GenerateThumbnailsResult(true, generated);
    }
}
