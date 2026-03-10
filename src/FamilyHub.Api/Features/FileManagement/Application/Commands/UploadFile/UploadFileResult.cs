using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;

public sealed record UploadFileResult(FileId FileId, StoredFile UploadedFile);
