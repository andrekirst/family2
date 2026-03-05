namespace FamilyHub.Api.Common.Search;

public sealed record CommandDescriptor(
    string Label,
    string Description,
    string[] Keywords,
    string Route,
    string[] RequiredPermissions,
    string Icon,
    string Group,
    string? LabelDe = null,
    string? DescriptionDe = null);
