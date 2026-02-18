using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFolder;

public sealed class RenameFolderCommandValidator : AbstractValidator<RenameFolderCommand>
{
    public RenameFolderCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FolderId).NotNull();
        RuleFor(x => x.NewName).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.RenamedBy).NotNull();
    }
}
