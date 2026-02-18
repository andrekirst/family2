using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetFile;

public sealed record GetFileQuery(
    FileId FileId,
    FamilyId FamilyId
) : IQuery<StoredFileDto?>;
