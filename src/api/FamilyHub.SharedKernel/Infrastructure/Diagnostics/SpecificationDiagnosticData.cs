namespace FamilyHub.SharedKernel.Infrastructure.Diagnostics;

/// <summary>
/// Data for the SpecificationEvaluated diagnostic event.
/// </summary>
/// <param name="SpecificationType">The type name of the specification.</param>
/// <param name="EntityType">The type name of the entity being queried.</param>
/// <param name="ElapsedMilliseconds">Time taken to apply the specification.</param>
/// <param name="IgnoredQueryFilters">Whether global query filters were ignored.</param>
/// <param name="IncludeCount">Number of Include expressions applied.</param>
/// <param name="HasOrdering">Whether ordering was applied.</param>
/// <param name="HasPagination">Whether pagination was applied.</param>
/// <param name="Timestamp">When the evaluation occurred.</param>
public sealed record SpecificationEvaluatedData(
    string SpecificationType,
    string EntityType,
    double ElapsedMilliseconds,
    bool IgnoredQueryFilters,
    int IncludeCount,
    bool HasOrdering,
    bool HasPagination,
    DateTime Timestamp);

/// <summary>
/// Data for the SpecificationFailed diagnostic event.
/// </summary>
/// <param name="SpecificationType">The type name of the specification.</param>
/// <param name="EntityType">The type name of the entity being queried.</param>
/// <param name="ErrorMessage">The error message.</param>
/// <param name="ExceptionType">The exception type name.</param>
/// <param name="Timestamp">When the failure occurred.</param>
public sealed record SpecificationFailedData(
    string SpecificationType,
    string EntityType,
    string ErrorMessage,
    string ExceptionType,
    DateTime Timestamp);

/// <summary>
/// Data for the CompositeSpecificationCreated diagnostic event.
/// </summary>
/// <param name="CompositeType">The composite specification type (And/Or/Not).</param>
/// <param name="LeftSpecificationType">The left operand specification type.</param>
/// <param name="RightSpecificationType">The right operand specification type (null for Not).</param>
/// <param name="EntityType">The entity type.</param>
/// <param name="Timestamp">When the composite was created.</param>
public sealed record CompositeSpecificationCreatedData(
    string CompositeType,
    string LeftSpecificationType,
    string? RightSpecificationType,
    string EntityType,
    DateTime Timestamp);

/// <summary>
/// Data for the ExpressionCompiled diagnostic event.
/// </summary>
/// <param name="SpecificationType">The specification type.</param>
/// <param name="EntityType">The entity type.</param>
/// <param name="CompilationMilliseconds">Time taken to compile the expression.</param>
/// <param name="Timestamp">When compilation occurred.</param>
public sealed record ExpressionCompiledData(
    string SpecificationType,
    string EntityType,
    double CompilationMilliseconds,
    DateTime Timestamp);
