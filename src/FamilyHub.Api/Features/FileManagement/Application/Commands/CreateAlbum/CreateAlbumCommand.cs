using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateAlbum;

public sealed record CreateAlbumCommand(
    AlbumName Name,
    string? Description,
    FamilyId FamilyId,
    UserId CreatedBy
) : ICommand<CreateAlbumResult>;
