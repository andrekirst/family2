using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Specification wrapper for raw LINQ expressions.
/// Use this for combining specifications with ad-hoc expressions.
/// </summary>
/// <typeparam name="T">The entity type being evaluated.</typeparam>
/// <param name="expression">The raw LINQ expression to wrap.</param>
/// <remarks>
/// This is an "escape hatch" for edge cases where creating a full specification class
/// would be overkill. Prefer named specifications for reusable business rules.
/// </remarks>
/// <example>
/// <code>
/// var spec = new ActiveUserSpecification()
///     .And(new RawExpressionSpecification&lt;User&gt;(u => u.CreatedAt > startDate));
/// </code>
/// </example>
public sealed class RawExpressionSpecification<T>(Expression<Func<T, bool>> expression) : Specification<T>
    where T : class
{
    private readonly Expression<Func<T, bool>> _expression = expression ?? throw new ArgumentNullException(nameof(expression));

    /// <inheritdoc/>
    public override Expression<Func<T, bool>> ToExpression() => _expression;
}
