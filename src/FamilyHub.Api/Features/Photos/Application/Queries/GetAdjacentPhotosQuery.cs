using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Queries;

public sealed record GetAdjacentPhotosQuery(
    PhotoId CurrentPhotoId,
    DateTime CurrentCreatedAt
) : IReadOnlyQuery<AdjacentPhotosDto>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
