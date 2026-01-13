namespace FamilyHub.SharedKernel.Infrastructure.Diagnostics;

/// <summary>
/// Defines diagnostic event names for specification operations.
/// Subscribe to these events for monitoring and debugging specification evaluation.
/// </summary>
public static class SpecificationDiagnosticEvents
{
    /// <summary>
    /// The diagnostic listener name for specification events.
    /// </summary>
    public const string ListenerName = "FamilyHub.Specifications";

    /// <summary>
    /// Event raised when a specification is evaluated against a queryable source.
    /// </summary>
    public const string SpecificationEvaluated = "FamilyHub.Specification.Evaluated";

    /// <summary>
    /// Event raised when specification evaluation fails.
    /// </summary>
    public const string SpecificationFailed = "FamilyHub.Specification.Failed";

    /// <summary>
    /// Event raised when a composite specification is created (And/Or/Not).
    /// </summary>
    public const string CompositeSpecificationCreated = "FamilyHub.Specification.CompositeCreated";

    /// <summary>
    /// Event raised when a specification's expression is compiled.
    /// </summary>
    public const string ExpressionCompiled = "FamilyHub.Specification.ExpressionCompiled";
}
