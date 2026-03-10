using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;

public sealed class CreateTagCommandHandler(
    ITagRepository tagRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateTagCommand, Result<CreateTagResult>>
{
    public async ValueTask<Result<CreateTagResult>> Handle(
        CreateTagCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var existing = await tagRepository.GetByNameAsync(command.Name, command.FamilyId, cancellationToken);
        if (existing is not null)
        {
            return DomainError.Conflict(DomainErrorCodes.Conflict, "A tag with this name already exists");
        }

        var tag = Tag.Create(command.Name, command.Color, command.FamilyId, command.UserId, utcNow);
        await tagRepository.AddAsync(tag, cancellationToken);

        return new CreateTagResult(tag.Id, tag);
    }
}
