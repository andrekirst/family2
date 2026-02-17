using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;

public sealed class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    private static readonly HashSet<string> AllowedMimeTypes =
        ["image/jpeg", "image/png", "image/webp"];

    public UploadAvatarCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .WithMessage("User ID is required");

        RuleFor(x => x.ImageData)
            .NotEmpty()
            .WithMessage("Image data is required");

        RuleFor(x => x.ImageData)
            .Must(data => data.Length <= 5 * 1024 * 1024)
            .When(x => x.ImageData.Length > 0)
            .WithMessage("Image must not exceed 5 MB");

        RuleFor(x => x.MimeType)
            .Must(mime => AllowedMimeTypes.Contains(mime.ToLowerInvariant()))
            .WithMessage("Only JPEG, PNG, and WebP images are supported");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required");
    }
}
