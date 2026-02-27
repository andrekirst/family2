using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Queries;

public sealed record GetPhotoQuery(
    PhotoId PhotoId
) : IQuery<PhotoDto?>;
