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
            .MustAsync(async (fileId, ct) =>
                await storedFileRepository.ExistsByIdAsync(fileId, ct))
            .WithErrorCode(DomainErrorCodes.FileNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FileNotFound].Value);

        RuleFor(x => x.TargetFolderId)
            .MustAsync(async (targetFolderId, ct) =>
                await folderRepository.ExistsByIdAsync(targetFolderId, ct))
            .WithErrorCode(DomainErrorCodes.FolderNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FolderNotFound].Value);
    }
}
