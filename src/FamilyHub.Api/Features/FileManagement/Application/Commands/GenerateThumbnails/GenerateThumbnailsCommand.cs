using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.GenerateThumbnails;

public sealed record GenerateThumbnailsCommand(
    FileId FileId,
    FamilyId FamilyId
) : ICommand<GenerateThumbnailsResult>;
