using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetTags;

public sealed record GetTagsQuery(
    FamilyId FamilyId
) : IReadOnlyQuery<List<TagDto>>, IFamilyScoped;
