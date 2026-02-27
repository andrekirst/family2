using FamilyHub.Api.Features.Photos.Application.Commands;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Photos.Application.Validators;

public sealed class UploadPhotoCommandValidator : AbstractValidator<UploadPhotoCommand>
{
    public UploadPhotoCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FamilyId)
            .NotNull()
            .WithMessage(_ => localizer["FamilyIdRequired"]);

        RuleFor(x => x.UploadedBy)
            .NotNull()
            .WithMessage(_ => localizer["CreatorIdRequired"]);

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0.");

        RuleFor(x => x.StoragePath)
            .NotEmpty()
            .WithMessage("Storage path is required.");
    }
}
