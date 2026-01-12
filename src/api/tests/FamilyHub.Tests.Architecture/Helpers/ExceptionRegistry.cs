namespace FamilyHub.Tests.Architecture.Helpers;

/// <summary>
/// Registry of documented architecture rule exceptions.
/// Each exception must have a phase, reason, ticket, and planned removal.
/// This ensures technical debt is tracked and eventually resolved.
/// </summary>
public static class ExceptionRegistry
{
    /// <summary>
    /// Represents a documented architecture exception with tracking metadata.
    /// </summary>
    /// <param name="Phase">The phase during which this exception was introduced (e.g., "Phase 5")</param>
    /// <param name="Reason">Why this exception exists</param>
    /// <param name="Ticket">The GitHub issue or ADR reference</param>
    /// <param name="PlannedRemoval">When this exception should be resolved</param>
    public sealed record ExceptionReason(
        string Phase,
        string Reason,
        string Ticket,
        string PlannedRemoval);

    /// <summary>
    /// Module boundary exceptions - cross-module dependencies that are temporarily allowed.
    /// Phase 3 coupling: Auth module physically hosts Family module implementations (logically separate).
    /// See ADR-005 for the Family Module Extraction Pattern.
    /// </summary>
    public static readonly Dictionary<string, ExceptionReason> ModuleBoundaryExceptions = new()
    {
        // Domain Layer - Phase 5 migration
        ["FamilyHub.Modules.Auth.Domain.User"] = new ExceptionReason(
            Phase: "Phase 5",
            Reason: "GetRoleInFamily() method accepts FamilyAggregate parameter for role resolution",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 6 - Full module separation with domain events"),

        // Presentation Layer - Phase 3 coupling (GraphQL type extensions)
        ["FamilyHub.Modules.Auth.Presentation.GraphQL.Types.FamilyTypeExtensions"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "GraphQL type extensions for Family types hosted in Auth module during transition",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 5 - Move to Family module presentation layer"),
        ["FamilyHub.Modules.Auth.Presentation.GraphQL.Types.InvitationsTypeExtensions"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Invitation types depend on Family aggregate for family context",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 5 - Decouple via FamilyId value objects"),
        ["FamilyHub.Modules.Auth.Presentation.GraphQL.Types.UserTypeExtensions"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "User type extensions include family membership information",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 5 - Use read models instead of direct references"),
        ["FamilyHub.Modules.Auth.Presentation.GraphQL.Queries.InvitationQueries"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Invitation queries return Family aggregate information",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 5 - Move to Family module"),
        ["FamilyHub.Modules.Auth.Presentation.GraphQL.Mappers.InvitationMapper"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Maps invitation entities including Family references",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 5 - Move to Family module"),

        // Infrastructure Layer
        ["FamilyHub.Modules.Auth.Infrastructure.BackgroundJobs.ExpiredInvitationCleanupJob"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Background job handles invitation cleanup with Family context",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 5 - Move to Family module infrastructure"),

        // Application Layer - Query Handlers
        ["FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies.GetUserFamiliesQueryHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Query handler returns Family aggregates for user",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module application layer"),
        ["FamilyHub.Modules.Auth.Application.Queries.GetPendingInvitations.GetPendingInvitationsQueryHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Query handler includes Family information in results",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module application layer"),
        ["FamilyHub.Modules.Auth.Application.Queries.GetPendingInvitations.PendingInvitationDto"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "DTO includes Family aggregate references",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module"),
        ["FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken.GetInvitationByTokenQueryHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Query handler accesses Family aggregate for context",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module application layer"),
        ["FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken.GetInvitationByTokenQueryValidator"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Validator references Family types for validation",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module"),
        ["FamilyHub.Modules.Auth.Application.Queries.GetInvitationByToken.GetInvitationByTokenResult"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Result type includes Family aggregate references",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module"),

        // Application Layer - Command Handlers
        ["FamilyHub.Modules.Auth.Application.Commands.UpdateInvitationRole.UpdateInvitationRoleCommandHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Command handler updates Family membership role",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module application layer"),
        ["FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail.InviteFamilyMemberByEmailCommandHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Command handler creates invitations with Family context",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module application layer"),
        ["FamilyHub.Modules.Auth.Application.Commands.InviteFamilyMemberByEmail.InviteFamilyMemberByEmailResult"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Result type includes Family references",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module"),
        ["FamilyHub.Modules.Auth.Application.Commands.CreateFamily.CreateFamilyCommandHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Command handler creates Family aggregate - core Family functionality",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module application layer"),
        ["FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin.CompleteZitadelLoginCommandHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Login handler accesses Family memberships for user context",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 5 - Use domain events for family context"),
        ["FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation.AcceptInvitationCommandHandler"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Command handler updates Family membership on invitation acceptance",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module application layer"),
        ["FamilyHub.Modules.Auth.Application.Commands.AcceptInvitation.AcceptInvitationCommandValidator"] = new ExceptionReason(
            Phase: "Phase 3",
            Reason: "Validator accesses Family aggregate for validation",
            Ticket: "ADR-005",
            PlannedRemoval: "Phase 4 - Move to Family module")
    };

    /// <summary>
    /// DDD pattern exceptions - types that temporarily don't follow DDD patterns.
    /// </summary>
    public static readonly Dictionary<string, ExceptionReason> DddPatternExceptions = new()
    {
        // Currently empty - all DDD patterns are enforced strictly
    };

    /// <summary>
    /// CQRS pattern exceptions - commands/queries that temporarily don't follow patterns.
    /// </summary>
    public static readonly Dictionary<string, ExceptionReason> CqrsPatternExceptions = new()
    {
        // Currently empty - all CQRS patterns are enforced strictly
    };

    /// <summary>
    /// Naming convention exceptions - types with non-standard names.
    /// </summary>
    public static readonly Dictionary<string, ExceptionReason> NamingConventionExceptions = new()
    {
        // Currently empty - all naming conventions are enforced strictly
    };

    /// <summary>
    /// Gets all exception registries for validation testing.
    /// </summary>
    public static IEnumerable<(string Category, Dictionary<string, ExceptionReason> Exceptions)> GetAllExceptionRegistries()
    {
        yield return ("ModuleBoundary", ModuleBoundaryExceptions);
        yield return ("DddPattern", DddPatternExceptions);
        yield return ("CqrsPattern", CqrsPatternExceptions);
        yield return ("NamingConvention", NamingConventionExceptions);
    }

    /// <summary>
    /// Filters out known violations from a list of failing type names.
    /// </summary>
    public static List<string> FilterKnownViolations(
        IEnumerable<string>? failingTypeNames,
        Dictionary<string, ExceptionReason> knownExceptions)
    {
        if (failingTypeNames == null)
        {
            return [];
        }

        return failingTypeNames
            .Where(t => !knownExceptions.ContainsKey(t))
            .ToList();
    }

    /// <summary>
    /// Builds a detailed exception message including known violations context.
    /// </summary>
    public static string BuildExceptionMessage(
        string ruleName,
        List<string> unexpectedViolations,
        Dictionary<string, ExceptionReason> knownExceptions)
    {
        var message = $"Rule '{ruleName}' detected unexpected violations. " +
                      $"Unexpected: [{string.Join(", ", unexpectedViolations)}]. ";

        if (knownExceptions.Count > 0)
        {
            var knownViolationsSummary = string.Join("; ",
                knownExceptions.Select(kv =>
                    $"{kv.Key} ({kv.Value.Phase}: {kv.Value.Reason})"));
            message += $"Known exceptions: [{knownViolationsSummary}]";
        }

        return message;
    }
}
