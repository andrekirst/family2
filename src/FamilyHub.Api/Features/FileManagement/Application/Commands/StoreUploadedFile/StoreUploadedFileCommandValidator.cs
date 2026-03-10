using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;

public sealed class StoreUploadedFileCommandValidator : AbstractValidator<StoreUploadedFileCommand>
{
    public StoreUploadedFileCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FileStream).NotNull();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UserId).NotNull();
    }
}
