using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSavedSearch;

public sealed record DeleteSavedSearchCommand(
    SavedSearchId SearchId,
    UserId UserId,
    FamilyId FamilyId
) : ICommand<DeleteSavedSearchResult>, IFamilyScoped;
