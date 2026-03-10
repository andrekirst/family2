using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

public sealed record MoveFileResult(FileId FileId, StoredFile MovedFile);
