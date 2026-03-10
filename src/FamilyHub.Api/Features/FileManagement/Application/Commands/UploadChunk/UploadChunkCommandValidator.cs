using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;

public sealed class UploadChunkCommandValidator : AbstractValidator<UploadChunkCommand>
{
    public UploadChunkCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.UploadId).NotEmpty();
        RuleFor(x => x.ChunkIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ChunkSize).GreaterThan(0);
        RuleFor(x => x.FamilyId).NotNull();
        RuleFor(x => x.UserId).NotNull();
    }
}
