using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;

public sealed record RenameFileResult(FileId FileId, StoredFile RenamedFile);
