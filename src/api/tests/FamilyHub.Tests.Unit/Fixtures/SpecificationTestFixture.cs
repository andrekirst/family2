using FamilyHub.SharedKernel.Domain.Specifications;
using FluentAssertions;
using FluentAssertions.Execution;

namespace FamilyHub.Tests.Unit.Fixtures;

/// <summary>
/// Test fixture for specification pattern tests.
/// Provides fluent assertions and helper methods for testing specifications.
/// </summary>
/// <typeparam name="TEntity">The entity type the specification targets.</typeparam>
public class SpecificationTestFixture<TEntity> where TEntity : class
{
    private readonly List<TEntity> _testEntities = [];
    private readonly List<AssertionResult> _assertionResults = [];

    /// <summary>
    /// Gets the test entities registered with this fixture.
    /// </summary>
    public IReadOnlyList<TEntity> TestEntities => _testEntities;

    /// <summary>
    /// Gets the assertion results from the last operation.
    /// </summary>
    public IReadOnlyList<AssertionResult> AssertionResults => _assertionResults;

    /// <summary>
    /// Registers a test entity with the fixture.
    /// </summary>
    /// <param name="entity">The entity to register.</param>
    /// <returns>This fixture for fluent chaining.</returns>
    public SpecificationTestFixture<TEntity> WithEntity(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _testEntities.Add(entity);
        return this;
    }

    /// <summary>
    /// Registers multiple test entities with the fixture.
    /// </summary>
    /// <param name="entities">The entities to register.</param>
    /// <returns>This fixture for fluent chaining.</returns>
    public SpecificationTestFixture<TEntity> WithEntities(params TEntity[] entities)
    {
        foreach (var entity in entities)
        {
            WithEntity(entity);
        }
        return this;
    }

    /// <summary>
    /// Asserts that the entity satisfies the specification.
    /// </summary>
    /// <param name="entity">The entity to test.</param>
    /// <param name="specification">The specification to evaluate.</param>
    /// <param name="because">Reason for the assertion.</param>
    public void ShouldSatisfy(TEntity entity, ISpecification<TEntity> specification, string? because = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(specification);

        var result = specification.IsSatisfiedBy(entity);
        result.Should().BeTrue(because ?? $"Entity should satisfy {specification.GetType().Name}");
    }

    /// <summary>
    /// Asserts that the entity does not satisfy the specification.
    /// </summary>
    /// <param name="entity">The entity to test.</param>
    /// <param name="specification">The specification to evaluate.</param>
    /// <param name="because">Reason for the assertion.</param>
    public void ShouldNotSatisfy(TEntity entity, ISpecification<TEntity> specification, string? because = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(specification);

        var result = specification.IsSatisfiedBy(entity);
        result.Should().BeFalse(because ?? $"Entity should not satisfy {specification.GetType().Name}");
    }

    /// <summary>
    /// Asserts that the specification matches exactly the expected entities.
    /// </summary>
    /// <param name="specification">The specification to evaluate.</param>
    /// <param name="expected">The expected matching entities.</param>
    public void ShouldMatchExactly(ISpecification<TEntity> specification, params TEntity[] expected)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var matched = _testEntities.Where(specification.IsSatisfiedBy).ToList();
        matched.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering(),
            $"Specification {specification.GetType().Name} should match exactly the expected entities");
    }

    /// <summary>
    /// Asserts that the specification matches the expected count of entities.
    /// </summary>
    /// <param name="specification">The specification to evaluate.</param>
    /// <param name="expectedCount">The expected count of matching entities.</param>
    public void ShouldMatchCount(ISpecification<TEntity> specification, int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var matchedCount = _testEntities.Count(specification.IsSatisfiedBy);
        matchedCount.Should().Be(expectedCount,
            $"Specification {specification.GetType().Name} should match {expectedCount} entities");
    }

    /// <summary>
    /// Asserts that the queryable specification compiles to a valid expression.
    /// </summary>
    /// <param name="specification">The queryable specification to test.</param>
    public void ShouldCompileExpression(IQueryableSpecification<TEntity> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        // Test that the expression can be obtained without throwing
        var expression = specification.ToExpression();
        expression.Should().NotBeNull($"Specification {specification.GetType().Name} should produce a valid expression");

        // Test that the expression can be compiled to a delegate
        var compiledFunc = expression.Compile();
        compiledFunc.Should().NotBeNull($"Expression from {specification.GetType().Name} should compile successfully");
    }

    /// <summary>
    /// Asserts that the specification's expression can be evaluated against test entities.
    /// </summary>
    /// <param name="specification">The queryable specification to test.</param>
    public void ShouldEvaluateExpression(IQueryableSpecification<TEntity> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var expression = specification.ToExpression();
        var compiledFunc = expression.Compile();

        // Ensure we can evaluate against all test entities without throwing
        using var scope = new AssertionScope();
        foreach (var entity in _testEntities)
        {
            var act = () => compiledFunc(entity);
            act.Should().NotThrow($"Expression from {specification.GetType().Name} should evaluate against entities");
        }
    }

    /// <summary>
    /// Asserts that two specifications are equivalent (produce same results for test entities).
    /// </summary>
    /// <param name="spec1">The first specification.</param>
    /// <param name="spec2">The second specification.</param>
    public void ShouldBeEquivalent(ISpecification<TEntity> spec1, ISpecification<TEntity> spec2)
    {
        ArgumentNullException.ThrowIfNull(spec1);
        ArgumentNullException.ThrowIfNull(spec2);

        var results1 = _testEntities.Where(spec1.IsSatisfiedBy).ToList();
        var results2 = _testEntities.Where(spec2.IsSatisfiedBy).ToList();

        results1.Should().BeEquivalentTo(results2,
            $"Specifications {spec1.GetType().Name} and {spec2.GetType().Name} should be equivalent");
    }

    /// <summary>
    /// Tests the specification against all registered entities and returns results.
    /// </summary>
    /// <param name="specification">The specification to test.</param>
    /// <returns>A dictionary of entities and their satisfaction results.</returns>
    public Dictionary<TEntity, bool> EvaluateAll(ISpecification<TEntity> specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return _testEntities.ToDictionary(
            entity => entity,
            entity => specification.IsSatisfiedBy(entity));
    }

    /// <summary>
    /// Clears all registered entities and assertion results.
    /// </summary>
    /// <returns>This fixture for fluent chaining.</returns>
    public SpecificationTestFixture<TEntity> Clear()
    {
        _testEntities.Clear();
        _assertionResults.Clear();
        return this;
    }
}

/// <summary>
/// Represents the result of an assertion operation.
/// </summary>
/// <param name="Entity">The entity that was tested.</param>
/// <param name="SpecificationName">The name of the specification.</param>
/// <param name="IsSatisfied">Whether the entity satisfied the specification.</param>
/// <param name="Expected">The expected result.</param>
public record AssertionResult(
    object Entity,
    string SpecificationName,
    bool IsSatisfied,
    bool Expected);
