using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.InitiateChunkedUpload;

public sealed class InitiateChunkedUploadCommandValidator : AbstractValidator<InitiateChunkedUploadCommand>
{
    public InitiateChunkedUploadCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UserId).NotNull();
    }
}
