using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Queries;

public sealed record GetAdjacentPhotosQuery(
    FamilyId FamilyId,
    PhotoId CurrentPhotoId,
    DateTime CurrentCreatedAt
) : IQuery<AdjacentPhotosDto>;
