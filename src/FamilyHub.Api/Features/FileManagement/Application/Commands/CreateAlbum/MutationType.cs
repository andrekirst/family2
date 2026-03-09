using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateAlbum;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> CreateAlbum(
        CreateAlbumRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new CreateAlbumCommand(
            AlbumName.From(input.Name),
            input.Description);

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => success,
            error => MutationError.FromDomainError(error));
    }
}
