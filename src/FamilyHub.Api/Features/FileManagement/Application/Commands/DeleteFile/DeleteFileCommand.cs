using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFile;

public sealed record DeleteFileCommand(
    FileId FileId,
    FamilyId FamilyId,
    UserId DeletedBy
) : ICommand<DeleteFileResult>;
