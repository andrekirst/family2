using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AddFileToAlbum;

public sealed record AddFileToAlbumCommand(
    AlbumId AlbumId,
    FileId FileId
) : ICommand<AddFileToAlbumResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
