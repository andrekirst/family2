using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AddFileToAlbum;

public sealed record AddFileToAlbumCommand(
    AlbumId AlbumId,
    FileId FileId,
    FamilyId FamilyId,
    UserId AddedBy
) : ICommand<AddFileToAlbumResult>;
