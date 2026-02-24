using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteAlbum;

public sealed record DeleteAlbumCommand(
    AlbumId AlbumId,
    FamilyId FamilyId
) : ICommand<DeleteAlbumResult>;
