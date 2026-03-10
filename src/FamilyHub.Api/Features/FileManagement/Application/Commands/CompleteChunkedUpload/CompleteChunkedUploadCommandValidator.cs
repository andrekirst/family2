using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CompleteChunkedUpload;

public sealed class CompleteChunkedUploadCommandValidator : AbstractValidator<CompleteChunkedUploadCommand>
{
    public CompleteChunkedUploadCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.UploadId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UserId).NotNull();
    }
}
