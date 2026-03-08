using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SaveSearch;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<SaveSearchResult> SaveSearch(
        string name,
        string query,
        string? filtersJson,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new SaveSearchCommand(name, query, filtersJson);
        return await commandBus.SendAsync(command, cancellationToken);
    }
}
