using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

public sealed class MoveFileCommandValidator : AbstractValidator<MoveFileCommand>
{
    public MoveFileCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FileId).NotNull();
        RuleFor(x => x.TargetFolderId).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.MovedBy).NotNull();
    }
}
