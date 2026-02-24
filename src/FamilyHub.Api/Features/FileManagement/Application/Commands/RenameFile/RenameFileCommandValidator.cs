using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;

public sealed class RenameFileCommandValidator : AbstractValidator<RenameFileCommand>
{
    public RenameFileCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FileId).NotNull();
        RuleFor(x => x.NewName).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.RenamedBy).NotNull();
    }
}
