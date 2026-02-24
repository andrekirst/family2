using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SaveSearch;

public sealed record SaveSearchCommand(
    string Name,
    string Query,
    string? FiltersJson,
    UserId UserId
) : ICommand<SaveSearchResult>;
