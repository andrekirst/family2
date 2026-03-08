using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;

public sealed class DeleteFolderBusinessValidator : AbstractValidator<DeleteFolderCommand>, IBusinessValidator<DeleteFolderCommand>
{
    public DeleteFolderBusinessValidator(
        IFolderRepository folderRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.FolderId)
            .MustAsync(async (folderId, ct) =>
            {
                var folder = await folderRepository.GetByIdAsync(folderId, ct);
                return folder is not null;
            })
            .WithErrorCode(DomainErrorCodes.FolderNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FolderNotFound].Value);
    }
}
