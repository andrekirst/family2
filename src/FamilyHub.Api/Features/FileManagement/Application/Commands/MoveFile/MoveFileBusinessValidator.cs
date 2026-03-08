using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

public sealed class MoveFileBusinessValidator : AbstractValidator<MoveFileCommand>, IBusinessValidator<MoveFileCommand>
{
    public MoveFileBusinessValidator(
        IStoredFileRepository storedFileRepository,
        IFolderRepository folderRepository,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x.FileId)
            .MustAsync(async (fileId, cancellationToken) =>
                await storedFileRepository.ExistsByIdAsync(fileId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.FileNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FileNotFound].Value);

        RuleFor(x => x.TargetFolderId)
            .MustAsync(async (targetFolderId, cancellationToken) =>
                await folderRepository.ExistsByIdAsync(targetFolderId, cancellationToken))
            .WithErrorCode(DomainErrorCodes.FolderNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FolderNotFound].Value);
    }
}
