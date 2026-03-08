using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSavedSearch;

public sealed record DeleteSavedSearchCommand(
    SavedSearchId SearchId
) : ICommand<DeleteSavedSearchResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
