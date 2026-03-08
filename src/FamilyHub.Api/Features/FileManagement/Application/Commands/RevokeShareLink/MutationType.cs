using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<RevokeShareLinkResult> RevokeShareLink(
        Guid shareLinkId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RevokeShareLinkCommand(
            ShareLinkId.From(shareLinkId));

        return await commandBus.SendAsync(command, cancellationToken);
    }
}
