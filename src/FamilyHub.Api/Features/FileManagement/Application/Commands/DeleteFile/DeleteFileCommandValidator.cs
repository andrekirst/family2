using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFile;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FileId).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.DeletedBy).NotNull();
    }
}
