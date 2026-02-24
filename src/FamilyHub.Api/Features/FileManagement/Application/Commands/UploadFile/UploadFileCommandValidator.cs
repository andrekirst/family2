using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Name).NotNull();
        RuleFor(x => x.MimeType).NotNull();
        RuleFor(x => x.StorageKey).NotNull();
        RuleFor(x => x.Checksum).NotNull();
        RuleFor(x => x.FolderId).NotNull();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UploadedBy).NotNull();
    }
}
