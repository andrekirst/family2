using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFiles;

public sealed record GetFilesQuery(
    FolderId FolderId,
    FamilyId FamilyId
) : IQuery<List<StoredFileDto>>;
