using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemoveFileFromAlbum;

public sealed record RemoveFileFromAlbumCommand(
    AlbumId AlbumId,
    FileId FileId
) : ICommand<RemoveFileFromAlbumResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
