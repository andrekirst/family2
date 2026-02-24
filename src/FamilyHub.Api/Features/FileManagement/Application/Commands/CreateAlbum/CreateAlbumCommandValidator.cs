using FluentValidation;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateAlbum;

public sealed class CreateAlbumCommandValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.FamilyId).NotEmpty();
        RuleFor(x => x.CreatedBy).NotEmpty();
    }
}
