using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.TagFile;

public sealed record TagFileCommand(
    FileId FileId,
    TagId TagId,
    FamilyId FamilyId
) : ICommand<TagFileResult>;
