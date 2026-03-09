using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.GenerateThumbnails;

public sealed record GenerateThumbnailsCommand(
    FileId FileId
) : ICommand<Result<GenerateThumbnailsResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
