using FamilyHub.SharedKernel.Domain.Specifications;
using FluentAssertions;
using FluentAssertions.Execution;

namespace FamilyHub.Tests.Unit.Fixtures;

/// <summary>
/// Extension methods for fluent specification testing.
/// Provides entity-centric assertions for specifications.
/// </summary>
public static class SpecificationTestExtensions
{
    /// <summary>
    /// Asserts that the entity satisfies the given specification.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to test.</param>
    /// <param name="specification">The specification to evaluate.</param>
    /// <param name="because">Reason for the assertion.</param>
    /// <returns>The entity for fluent chaining.</returns>
    public static TEntity ShouldSatisfy<TEntity>(
        this TEntity entity,
        ISpecification<TEntity> specification,
        string? because = null) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(specification);

        var result = specification.IsSatisfiedBy(entity);
        result.Should().BeTrue(because ?? $"Entity should satisfy {specification.GetType().Name}");

        return entity;
    }

    /// <summary>
    /// Asserts that the entity does not satisfy the given specification.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to test.</param>
    /// <param name="specification">The specification to evaluate.</param>
    /// <param name="because">Reason for the assertion.</param>
    /// <returns>The entity for fluent chaining.</returns>
    public static TEntity ShouldNotSatisfy<TEntity>(
        this TEntity entity,
        ISpecification<TEntity> specification,
        string? because = null) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(specification);

        var result = specification.IsSatisfiedBy(entity);
        result.Should().BeFalse(because ?? $"Entity should not satisfy {specification.GetType().Name}");

        return entity;
    }

    /// <summary>
    /// Asserts that the entity satisfies all given specifications.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to test.</param>
    /// <param name="specifications">The specifications to evaluate.</param>
    /// <returns>The entity for fluent chaining.</returns>
    public static TEntity ShouldSatisfyAll<TEntity>(
        this TEntity entity,
        params ISpecification<TEntity>[] specifications) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var scope = new AssertionScope();
        foreach (var specification in specifications)
        {
            var result = specification.IsSatisfiedBy(entity);
            result.Should().BeTrue($"Entity should satisfy {specification.GetType().Name}");
        }

        return entity;
    }

    /// <summary>
    /// Asserts that the entity satisfies at least one of the given specifications.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to test.</param>
    /// <param name="specifications">The specifications to evaluate.</param>
    /// <returns>The entity for fluent chaining.</returns>
    public static TEntity ShouldSatisfyAny<TEntity>(
        this TEntity entity,
        params ISpecification<TEntity>[] specifications) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var satisfiesAny = specifications.Any(spec => spec.IsSatisfiedBy(entity));
        satisfiesAny.Should().BeTrue(
            $"Entity should satisfy at least one of: {string.Join(", ", specifications.Select(s => s.GetType().Name))}");

        return entity;
    }

    /// <summary>
    /// Asserts that the entity does not satisfy any of the given specifications.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to test.</param>
    /// <param name="specifications">The specifications to evaluate.</param>
    /// <returns>The entity for fluent chaining.</returns>
    public static TEntity ShouldNotSatisfyAny<TEntity>(
        this TEntity entity,
        params ISpecification<TEntity>[] specifications) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var scope = new AssertionScope();
        foreach (var specification in specifications)
        {
            var result = specification.IsSatisfiedBy(entity);
            result.Should().BeFalse($"Entity should not satisfy {specification.GetType().Name}");
        }

        return entity;
    }

    /// <summary>
    /// Creates a new specification test fixture for the entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entities">Initial entities to register.</param>
    /// <returns>A new specification test fixture.</returns>
    public static SpecificationTestFixture<TEntity> CreateSpecificationFixture<TEntity>(
        params TEntity[] entities) where TEntity : class
    {
        var fixture = new SpecificationTestFixture<TEntity>();
        fixture.WithEntities(entities);
        return fixture;
    }

    /// <summary>
    /// Filters entities by specification and returns matching entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>Entities satisfying the specification.</returns>
    public static IEnumerable<TEntity> Satisfying<TEntity>(
        this IEnumerable<TEntity> entities,
        ISpecification<TEntity> specification) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);
        return entities.Where(specification.IsSatisfiedBy);
    }

    /// <summary>
    /// Filters entities by specification and returns non-matching entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>Entities not satisfying the specification.</returns>
    public static IEnumerable<TEntity> NotSatisfying<TEntity>(
        this IEnumerable<TEntity> entities,
        ISpecification<TEntity> specification) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);
        return entities.Where(e => !specification.IsSatisfiedBy(e));
    }

    /// <summary>
    /// Asserts that a queryable specification's expression can be compiled and evaluated.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="specification">The specification to test.</param>
    public static void ShouldHaveValidExpression<TEntity>(
        this IQueryableSpecification<TEntity> specification) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);

        var expression = specification.ToExpression();
        expression.Should().NotBeNull($"Specification {specification.GetType().Name} should produce a valid expression");

        var compiledFunc = expression.Compile();
        compiledFunc.Should().NotBeNull($"Expression from {specification.GetType().Name} should compile successfully");
    }

    /// <summary>
    /// Asserts that the specification result matches the expected value.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="specification">The specification to evaluate.</param>
    /// <param name="entity">The entity to test.</param>
    /// <param name="expected">The expected result.</param>
    /// <param name="because">Reason for the assertion.</param>
    public static void ShouldEvaluateTo<TEntity>(
        this ISpecification<TEntity> specification,
        TEntity entity,
        bool expected,
        string? because = null) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(entity);

        var result = specification.IsSatisfiedBy(entity);
        result.Should().Be(expected,
            because ?? $"Specification {specification.GetType().Name} should evaluate to {expected}");
    }
}
