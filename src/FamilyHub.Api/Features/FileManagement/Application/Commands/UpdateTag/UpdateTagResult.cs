using FamilyHub.Common.Domain.ValueObjects;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;

public sealed record UpdateTagResult(TagId TagId, Tag UpdatedTag);
