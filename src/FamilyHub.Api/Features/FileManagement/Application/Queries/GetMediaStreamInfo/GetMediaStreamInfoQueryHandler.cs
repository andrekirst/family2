using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetMediaStreamInfo;

public sealed class GetMediaStreamInfoQueryHandler(
    IStoredFileRepository fileRepository,
    IFileThumbnailRepository thumbnailRepository)
    : IQueryHandler<GetMediaStreamInfoQuery, Result<MediaStreamInfoDto>>
{
    private static readonly HashSet<string> StreamableMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "video/mp4", "video/webm", "video/ogg",
        "audio/mpeg", "audio/ogg", "audio/wav", "audio/webm",
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf"
    };

    public async ValueTask<Result<MediaStreamInfoDto>> Handle(
        GetMediaStreamInfoQuery query,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(query.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        if (file.FamilyId != query.FamilyId)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found in this family");
        }

        var thumbnails = await thumbnailRepository.GetByFileIdAsync(query.FileId, cancellationToken);

        return new MediaStreamInfoDto
        {
            FileId = file.Id.Value,
            MimeType = file.MimeType.Value,
            FileSize = file.Size.Value,
            StorageKey = file.StorageKey.Value,
            SupportsRangeRequests = true,
            IsStreamable = StreamableMimeTypes.Contains(file.MimeType.Value),
            Thumbnails = thumbnails.Select(FileManagementMapper.ToDto).ToList()
        };
    }
}
