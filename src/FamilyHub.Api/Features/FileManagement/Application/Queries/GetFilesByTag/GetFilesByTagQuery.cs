using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFilesByTag;

public sealed record GetFilesByTagQuery(
    List<TagId> TagIds,
    FamilyId FamilyId
) : IQuery<List<StoredFileDto>>;
