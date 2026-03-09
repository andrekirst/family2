using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;

public sealed record MoveFolderCommand(
    FolderId FolderId,
    FolderId TargetParentFolderId
) : ICommand<Result<MoveFolderResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
