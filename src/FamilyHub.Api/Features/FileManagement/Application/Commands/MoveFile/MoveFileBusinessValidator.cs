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
            {
                var file = await storedFileRepository.GetByIdAsync(fileId, ct);
                return file is not null;
            })
            .WithErrorCode(DomainErrorCodes.FileNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FileNotFound].Value);

        RuleFor(x => x.TargetFolderId)
            .MustAsync(async (targetFolderId, ct) =>
            {
                var folder = await folderRepository.GetByIdAsync(targetFolderId, ct);
                return folder is not null;
            })
            .WithErrorCode(DomainErrorCodes.FolderNotFound)
            .WithMessage(_ => localizer[DomainErrorCodes.FolderNotFound].Value);
    }
}
