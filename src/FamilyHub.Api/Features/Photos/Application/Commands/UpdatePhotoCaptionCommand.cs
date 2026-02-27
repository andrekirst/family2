using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Application.Commands;

public sealed record UpdatePhotoCaptionCommand(
    PhotoId PhotoId,
    PhotoCaption? Caption
) : ICommand<UpdatePhotoCaptionResult>;
