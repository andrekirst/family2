using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameAlbum;

public sealed record RenameAlbumCommand(
    AlbumId AlbumId,
    AlbumName NewName
) : ICommand<Result<RenameAlbumResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
