using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetAlbums;

public sealed record GetAlbumsQuery(
    FamilyId FamilyId
) : IQuery<List<AlbumDto>>;
