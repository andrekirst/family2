using System.Linq.Expressions;

namespace FamilyHub.SharedKernel.Domain.Specifications;

/// <summary>
/// Extension methods for composing and extending specifications.
/// Provides fluent API for combining specifications with And, Or, Not operations.
/// </summary>
public static class SpecificationExtensions
{
    #region Composition Extensions

    /// <param name="left">The left specification.</param>
    /// <typeparam name="T">The entity type.</typeparam>
    extension<T>(Specification<T> left) where T : class
    {
        /// <summary>
        /// Combines this specification with another using logical AND.
        /// </summary>
        /// <param name="right">The right specification.</param>
        /// <returns>A new AndSpecification combining both.</returns>
        public Specification<T> And(Specification<T> right) => new AndSpecification<T>(left, right);

        /// <summary>
        /// Combines this specification with another using logical OR.
        /// </summary>
        /// <param name="right">The right specification.</param>
        /// <returns>A new OrSpecification combining both.</returns>
        public Specification<T> Or(Specification<T> right) => new OrSpecification<T>(left, right);

        /// <summary>
        /// Negates this specification.
        /// </summary>
        /// <returns>A new NotSpecification.</returns>
        public Specification<T> Not() => new NotSpecification<T>(left);

        /// <summary>
        /// Combines this specification with a raw expression using logical AND.
        /// </summary>
        /// <param name="right">The raw expression to combine.</param>
        /// <returns>A new AndSpecification combining both.</returns>
        public Specification<T> And(Expression<Func<T, bool>> right) => new AndSpecification<T>(left, new RawExpressionSpecification<T>(right));

        /// <summary>
        /// Combines this specification with a raw expression using logical OR.
        /// </summary>
        /// <param name="right">The raw expression to combine.</param>
        /// <returns>A new OrSpecification combining both.</returns>
        public Specification<T> Or(Expression<Func<T, bool>> right) => new OrSpecification<T>(left, new RawExpressionSpecification<T>(right));
    }

    #endregion

    #region Ordering Extensions

    /// <param name="spec">The specification to order.</param>
    /// <typeparam name="T">The entity type.</typeparam>
    extension<T>(Specification<T> spec) where T : class
    {
        /// <summary>
        /// Adds ascending ordering to this specification.
        /// </summary>
        /// <param name="keySelector">The property to order by.</param>
        /// <returns>A new OrderedSpecification.</returns>
        public OrderedSpecification<T> OrderBy(Expression<Func<T, object>> keySelector) => new(spec, keySelector, isDescending: false);

        /// <summary>
        /// Adds descending ordering to this specification.
        /// </summary>
        /// <param name="keySelector">The property to order by.</param>
        /// <returns>A new OrderedSpecification.</returns>
        public OrderedSpecification<T> OrderByDescending(Expression<Func<T, object>> keySelector) => new(spec, keySelector, isDescending: true);
    }

    /// <param name="spec">The specification to order.</param>
    /// <typeparam name="T">The entity type.</typeparam>
    extension<T>(IQueryableSpecification<T> spec) where T : class
    {
        /// <summary>
        /// Adds ascending ordering to this queryable specification.
        /// </summary>
        /// <param name="keySelector">The property to order by.</param>
        /// <returns>A new OrderedSpecification.</returns>
        public OrderedSpecification<T> OrderBy(Expression<Func<T, object>> keySelector) => new(spec, keySelector, isDescending: false);

        /// <summary>
        /// Adds descending ordering to this queryable specification.
        /// </summary>
        /// <param name="keySelector">The property to order by.</param>
        /// <returns>A new OrderedSpecification.</returns>
        public OrderedSpecification<T> OrderByDescending(Expression<Func<T, object>> keySelector) => new(spec, keySelector, isDescending: true);
    }

    #endregion

    #region Pagination Extensions

    /// <param name="spec">The ordered specification to paginate.</param>
    /// <typeparam name="T">The entity type.</typeparam>
    extension<T>(IOrderedSpecification<T> spec) where T : class
    {
        /// <summary>
        /// Adds pagination to this ordered specification.
        /// </summary>
        /// <param name="skip">Number of entities to skip.</param>
        /// <param name="take">Maximum number of entities to return.</param>
        /// <returns>A new PaginatedSpecification.</returns>
        public PaginatedSpecification<T> Paginate(int skip,
            int take) => new(spec, skip, take);

        /// <summary>
        /// Adds pagination for a specific page to this ordered specification.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A new PaginatedSpecification for the specified page.</returns>
        public PaginatedSpecification<T> Page(int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1.");
            }

            var skip = (pageNumber - 1) * pageSize;
            return new PaginatedSpecification<T>(spec, skip, pageSize);
        }
    }

    #endregion

    #region Evaluation Extensions

    /// <param name="spec">The specification to apply.</param>
    /// <typeparam name="T">The entity type.</typeparam>
    extension<T>(ISpecification<T> spec)
    {
        /// <summary>
        /// Filters a collection using this specification.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <returns>Entities satisfying the specification.</returns>
        public IEnumerable<T> Filter(IEnumerable<T> source)
            => source.Where(spec.IsSatisfiedBy);

        /// <summary>
        /// Checks if any entity in the collection satisfies this specification.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <returns>True if any entity satisfies the specification.</returns>
        public bool Any(IEnumerable<T> source)
            => source.Any(spec.IsSatisfiedBy);

        /// <summary>
        /// Checks if all entities in the collection satisfy this specification.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <returns>True if all entities satisfy the specification.</returns>
        public bool All(IEnumerable<T> source)
            => source.All(spec.IsSatisfiedBy);

        /// <summary>
        /// Counts entities satisfying this specification.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <returns>The count of matching entities.</returns>
        public int Count(IEnumerable<T> source)
            => source.Count(spec.IsSatisfiedBy);
    }

    /// <summary>
    /// Gets the first entity satisfying this specification, or null.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="spec">The specification to apply.</param>
    /// <param name="source">The source collection.</param>
    /// <returns>The first matching entity or null.</returns>
    public static T? FirstOrDefault<T>(this ISpecification<T> spec, IEnumerable<T> source)
        where T : class
        => source.FirstOrDefault(spec.IsSatisfiedBy);

    #endregion
}
