using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFileVersions;

public sealed record GetFileVersionsQuery(
    FileId FileId,
    FamilyId FamilyId
) : IQuery<List<FileVersionDto>>;
