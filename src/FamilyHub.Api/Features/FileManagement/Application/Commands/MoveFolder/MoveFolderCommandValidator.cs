using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;

public sealed class MoveFolderCommandValidator : AbstractValidator<MoveFolderCommand>
{
    public MoveFolderCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FolderId).NotNull();
        RuleFor(x => x.TargetParentFolderId).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.MovedBy).NotNull();
    }
}
