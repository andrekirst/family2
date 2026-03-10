using FamilyHub.Common.Domain.ValueObjects;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;

public sealed record CreateTagResult(TagId TagId, Tag CreatedTag);
