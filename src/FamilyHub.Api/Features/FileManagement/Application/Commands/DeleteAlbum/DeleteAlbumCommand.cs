using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteAlbum;

public sealed record DeleteAlbumCommand(
    AlbumId AlbumId
) : ICommand<Result<DeleteAlbumResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
