using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

public sealed class CreateConversationBusinessValidator : AbstractValidator<CreateConversationCommand>, IBusinessValidator<CreateConversationCommand>
{
    public CreateConversationBusinessValidator(
        IFolderRepository folderRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.FamilyId)
            .MustAsync(async (familyId, cancellationToken) =>
            {
                var rootFolder = await folderRepository.GetRootFolderAsync(familyId, cancellationToken);
                return rootFolder is not null;
            })
            .WithErrorCode(DomainErrorCodes.FolderNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FolderNotFound].Value);
    }
}
