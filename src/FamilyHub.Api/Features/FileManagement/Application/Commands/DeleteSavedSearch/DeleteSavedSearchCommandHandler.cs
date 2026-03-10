using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSavedSearch;

public sealed class DeleteSavedSearchCommandHandler(
    ISavedSearchRepository savedSearchRepository)
    : ICommandHandler<DeleteSavedSearchCommand, Result<DeleteSavedSearchResult>>
{
    public async ValueTask<Result<DeleteSavedSearchResult>> Handle(
        DeleteSavedSearchCommand command,
        CancellationToken cancellationToken)
    {
        var search = await savedSearchRepository.GetByIdAsync(command.SearchId, cancellationToken);
        if (search is null)
        {
            return DomainError.NotFound(DomainErrorCodes.NotFound, "Saved search not found");
        }

        if (search.UserId != command.UserId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Cannot delete another user's saved search");
        }

        await savedSearchRepository.RemoveAsync(search, cancellationToken);

        return new DeleteSavedSearchResult(true);
    }
}
