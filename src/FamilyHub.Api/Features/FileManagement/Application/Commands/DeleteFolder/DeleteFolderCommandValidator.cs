using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;

public sealed class DeleteFolderCommandValidator : AbstractValidator<DeleteFolderCommand>, IInputValidator<DeleteFolderCommand>
{
    public DeleteFolderCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FolderId).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UserId).NotNull();
    }
}
