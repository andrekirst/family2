using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Application.Commands;

public sealed record UpdatePhotoCaptionResult(
    PhotoId PhotoId
);
