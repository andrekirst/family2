using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Queries;

public sealed record GetPhotosQuery(
    FamilyId FamilyId,
    int Skip,
    int Take
) : IReadOnlyQuery<PhotosPageDto>, IFamilyScoped;
