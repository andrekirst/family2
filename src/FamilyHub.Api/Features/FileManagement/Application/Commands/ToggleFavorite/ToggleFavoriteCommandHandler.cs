using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleFavorite;

public sealed class ToggleFavoriteCommandHandler(
    IStoredFileRepository storedFileRepository,
    IUserFavoriteRepository userFavoriteRepository,
    TimeProvider timeProvider)
    : ICommandHandler<ToggleFavoriteCommand, Result<ToggleFavoriteResult>>
{
    public async ValueTask<Result<ToggleFavoriteResult>> Handle(
        ToggleFavoriteCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        if (file.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File belongs to a different family");
        }

        var isFavorited = await userFavoriteRepository.ExistsAsync(command.UserId, command.FileId, cancellationToken);

        if (isFavorited)
        {
            var favorites = await userFavoriteRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            var favorite = favorites.First(f => f.FileId == command.FileId);
            await userFavoriteRepository.RemoveAsync(favorite, cancellationToken);
            return new ToggleFavoriteResult(false);
        }
        else
        {
            var favorite = UserFavorite.Create(command.UserId, command.FileId, utcNow);
            await userFavoriteRepository.AddAsync(favorite, cancellationToken);
            return new ToggleFavoriteResult(true);
        }
    }
}
