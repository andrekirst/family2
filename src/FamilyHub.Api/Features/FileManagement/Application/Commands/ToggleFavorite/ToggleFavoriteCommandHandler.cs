using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleFavorite;

public sealed class ToggleFavoriteCommandHandler(
    IStoredFileRepository storedFileRepository,
    IUserFavoriteRepository userFavoriteRepository)
    : ICommandHandler<ToggleFavoriteCommand, ToggleFavoriteResult>
{
    public async ValueTask<ToggleFavoriteResult> Handle(
        ToggleFavoriteCommand command,
        CancellationToken cancellationToken)
    {
        var file = await storedFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        if (file.FamilyId != command.FamilyId)
            throw new DomainException("File belongs to a different family", DomainErrorCodes.Forbidden);

        var isFavorited = await userFavoriteRepository.ExistsAsync(command.UserId, command.FileId, cancellationToken);

        if (isFavorited)
        {
            // Unfavorite
            var favorites = await userFavoriteRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            var favorite = favorites.First(f => f.FileId == command.FileId);
            await userFavoriteRepository.RemoveAsync(favorite, cancellationToken);
            return new ToggleFavoriteResult(false);
        }
        else
        {
            // Favorite
            var favorite = UserFavorite.Create(command.UserId, command.FileId);
            await userFavoriteRepository.AddAsync(favorite, cancellationToken);
            return new ToggleFavoriteResult(true);
        }
    }
}
