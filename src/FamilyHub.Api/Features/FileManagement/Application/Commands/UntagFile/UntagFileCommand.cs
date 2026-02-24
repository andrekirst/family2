using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UntagFile;

public sealed record UntagFileCommand(
    FileId FileId,
    TagId TagId,
    FamilyId FamilyId
) : ICommand<UntagFileResult>;
