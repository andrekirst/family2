using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Application.Commands;

public sealed record DeletePhotoCommand(
    PhotoId PhotoId,
    UserId DeletedBy
) : ICommand<DeletePhotoResult>;
