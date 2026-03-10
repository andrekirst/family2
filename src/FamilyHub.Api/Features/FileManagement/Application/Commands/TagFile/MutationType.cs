using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.TagFile;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> TagFile(
        Guid fileId,
        Guid tagId,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new TagFileCommand(FileId.From(fileId), TagId.From(tagId));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match(
            success => true,
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
