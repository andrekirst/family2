using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SaveSearch;

public sealed class SaveSearchCommandHandler(
    ISavedSearchRepository savedSearchRepository,
    TimeProvider timeProvider)
    : ICommandHandler<SaveSearchCommand, SaveSearchResult>
{
    public async ValueTask<SaveSearchResult> Handle(
        SaveSearchCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var savedSearch = SavedSearch.Create(
            command.UserId,
            command.Name,
            command.Query,
            command.FiltersJson,
            utcNow);

        await savedSearchRepository.AddAsync(savedSearch, cancellationToken);

        return new SaveSearchResult(true, savedSearch.Id.Value);
    }
}
