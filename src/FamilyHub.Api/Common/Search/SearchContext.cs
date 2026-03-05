using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Search;

public sealed record SearchContext(
    UserId UserId,
    FamilyId? FamilyId,
    string Query,
    int Limit = 10,
    string? Locale = null);
