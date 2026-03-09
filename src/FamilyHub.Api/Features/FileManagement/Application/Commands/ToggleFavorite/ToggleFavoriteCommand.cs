using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleFavorite;

public sealed record ToggleFavoriteCommand(
    FileId FileId
) : ICommand<Result<ToggleFavoriteResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
