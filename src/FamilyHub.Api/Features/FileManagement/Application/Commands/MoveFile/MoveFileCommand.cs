using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

public sealed record MoveFileCommand(
    FileId FileId,
    FolderId TargetFolderId
) : ICommand<Result<MoveFileResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
