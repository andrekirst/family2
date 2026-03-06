using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Models;

namespace FamilyHub.Api.Features.Messaging.Application.Queries.GetFamilyMessages;

/// <summary>
/// Query to get messages for a family channel with cursor pagination.
/// </summary>
public sealed record GetFamilyMessagesQuery(
    FamilyId FamilyId,
    int Limit = 50,
    DateTime? Before = null
) : IReadOnlyQuery<List<MessageDto>>;
