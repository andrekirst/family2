using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSavedSearch;

public sealed class DeleteSavedSearchCommandHandler(
    ISavedSearchRepository savedSearchRepository)
    : ICommandHandler<DeleteSavedSearchCommand, DeleteSavedSearchResult>
{
    public async ValueTask<DeleteSavedSearchResult> Handle(
        DeleteSavedSearchCommand command,
        CancellationToken cancellationToken)
    {
        var search = await savedSearchRepository.GetByIdAsync(command.SearchId, cancellationToken)
            ?? throw new DomainException("Saved search not found", DomainErrorCodes.NotFound);

        if (search.UserId != command.UserId)
            throw new DomainException("Cannot delete another user's saved search", DomainErrorCodes.Forbidden);

        await savedSearchRepository.RemoveAsync(search, cancellationToken);

        return new DeleteSavedSearchResult(true);
    }
}
