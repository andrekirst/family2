using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;

public sealed class CreateTagCommandHandler(
    ITagRepository tagRepository)
    : ICommandHandler<CreateTagCommand, CreateTagResult>
{
    public async ValueTask<CreateTagResult> Handle(
        CreateTagCommand command,
        CancellationToken cancellationToken)
    {
        // Check for duplicate tag name within the family
        var existing = await tagRepository.GetByNameAsync(command.Name, command.FamilyId, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException("A tag with this name already exists", DomainErrorCodes.Conflict);
        }

        var tag = Tag.Create(command.Name, command.Color, command.FamilyId, command.CreatedBy);
        await tagRepository.AddAsync(tag, cancellationToken);

        return new CreateTagResult(tag.Id);
    }
}
