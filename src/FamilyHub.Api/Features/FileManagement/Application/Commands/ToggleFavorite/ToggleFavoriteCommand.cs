using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleFavorite;

public sealed record ToggleFavoriteCommand(
    FileId FileId,
    UserId UserId,
    FamilyId FamilyId
) : ICommand<ToggleFavoriteResult>;
