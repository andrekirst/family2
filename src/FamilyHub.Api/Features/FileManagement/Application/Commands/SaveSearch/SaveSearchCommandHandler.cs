using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SaveSearch;

public sealed class SaveSearchCommandHandler(
    ISavedSearchRepository savedSearchRepository)
    : ICommandHandler<SaveSearchCommand, SaveSearchResult>
{
    public async ValueTask<SaveSearchResult> Handle(
        SaveSearchCommand command,
        CancellationToken cancellationToken)
    {
        var savedSearch = SavedSearch.Create(
            command.UserId,
            command.Name,
            command.Query,
            command.FiltersJson);

        await savedSearchRepository.AddAsync(savedSearch, cancellationToken);

        return new SaveSearchResult(true, savedSearch.Id.Value);
    }
}
