using FamilyHub.SharedKernel.Domain.Specifications;
using FamilyHub.Tests.Unit.Fixtures;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.Specifications.SharedKernel;

/// <summary>
/// Unit tests for composite specification implementations.
/// Tests And, Or, Not specifications and operator overloads.
/// </summary>
public class CompositeSpecificationTests
{
    private record TestEntity(int Value, string Name, bool IsActive);

    private class ValueGreaterThanSpec(int threshold) : Specification<TestEntity>
    {
        private readonly int _threshold = threshold;

        public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression()
            => e => e.Value > _threshold;
    }

    private class IsActiveSpec : Specification<TestEntity>
    {
        public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression()
            => e => e.IsActive;
    }

    private class NameStartsWithSpec(string prefix) : Specification<TestEntity>
    {
        private readonly string _prefix = prefix;

        public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression()
            => e => e.Name.StartsWith(_prefix);
    }

    [Fact]
    public void AndSpecification_BothSatisfied_ReturnsTrue()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", true);

        // Act
        var combinedSpec = valueSpec.And(activeSpec);

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public void AndSpecification_OnlyFirstSatisfied_ReturnsFalse()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", false);

        // Act
        var combinedSpec = valueSpec.And(activeSpec);

        // Assert
        entity.ShouldNotSatisfy(combinedSpec);
    }

    [Fact]
    public void AndSpecification_OnlySecondSatisfied_ReturnsFalse()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(3, "Test", true);

        // Act
        var combinedSpec = valueSpec.And(activeSpec);

        // Assert
        entity.ShouldNotSatisfy(combinedSpec);
    }

    [Fact]
    public void AndSpecification_NeitherSatisfied_ReturnsFalse()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(3, "Test", false);

        // Act
        var combinedSpec = valueSpec.And(activeSpec);

        // Assert
        entity.ShouldNotSatisfy(combinedSpec);
    }

    [Fact]
    public void OrSpecification_BothSatisfied_ReturnsTrue()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", true);

        // Act
        var combinedSpec = valueSpec.Or(activeSpec);

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public void OrSpecification_OnlyFirstSatisfied_ReturnsTrue()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", false);

        // Act
        var combinedSpec = valueSpec.Or(activeSpec);

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public void OrSpecification_OnlySecondSatisfied_ReturnsTrue()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(3, "Test", true);

        // Act
        var combinedSpec = valueSpec.Or(activeSpec);

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public void OrSpecification_NeitherSatisfied_ReturnsFalse()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(3, "Test", false);

        // Act
        var combinedSpec = valueSpec.Or(activeSpec);

        // Assert
        entity.ShouldNotSatisfy(combinedSpec);
    }

    [Fact]
    public void NotSpecification_OriginalSatisfied_ReturnsFalse()
    {
        // Arrange
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", true);

        // Act
        var notSpec = activeSpec.Not();

        // Assert
        entity.ShouldNotSatisfy(notSpec);
    }

    [Fact]
    public void NotSpecification_OriginalNotSatisfied_ReturnsTrue()
    {
        // Arrange
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", false);

        // Act
        var notSpec = activeSpec.Not();

        // Assert
        entity.ShouldSatisfy(notSpec);
    }

    [Fact]
    public void OperatorAnd_BothSatisfied_ReturnsTrue()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", true);

        // Act
        var combinedSpec = valueSpec & activeSpec;

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public void OperatorOr_OnlyFirstSatisfied_ReturnsTrue()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", false);

        // Act
        var combinedSpec = valueSpec | activeSpec;

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public void OperatorNot_OriginalSatisfied_ReturnsFalse()
    {
        // Arrange
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "Test", true);

        // Act
        var notSpec = !activeSpec;

        // Assert
        entity.ShouldNotSatisfy(notSpec);
    }

    [Fact]
    public void ComplexComposition_MultipleLevels_EvaluatesCorrectly()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();
        var nameSpec = new NameStartsWithSpec("Test");

        var entity1 = new TestEntity(10, "TestItem", true);  // All satisfied
        var entity2 = new TestEntity(10, "Other", true);     // Value and Active satisfied
        var entity3 = new TestEntity(3, "TestItem", true);   // Name and Active satisfied
        var entity4 = new TestEntity(3, "Other", false);     // None satisfied

        // Act - (Value > 5 AND Active) OR Name starts with "Test"
        var complexSpec = (valueSpec & activeSpec) | nameSpec;

        // Assert
        entity1.ShouldSatisfy(complexSpec);
        entity2.ShouldSatisfy(complexSpec);
        entity3.ShouldSatisfy(complexSpec);
        entity4.ShouldNotSatisfy(complexSpec);
    }

    [Fact]
    public void RawExpressionSpecification_CombinesWithAnd_Works()
    {
        // Arrange
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "TestItem", true);

        // Act
        var combinedSpec = activeSpec.And(e => e.Value > 5);

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public void RawExpressionSpecification_CombinesWithOr_Works()
    {
        // Arrange
        var activeSpec = new IsActiveSpec();
        var entity = new TestEntity(10, "TestItem", false);

        // Act
        var combinedSpec = activeSpec.Or(e => e.Value > 5);

        // Assert
        entity.ShouldSatisfy(combinedSpec);
    }

    [Fact]
    public async Task Specification_IsSatisfiedByAsync_ReturnsCorrectResult()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var entity = new TestEntity(10, "Test", true);

        // Act
        var result = await valueSpec.IsSatisfiedByAsync(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Specification_Evaluate_ReturnsSuccessResult()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var entity = new TestEntity(10, "Test", true);

        // Act
        var result = valueSpec.Evaluate(entity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Specification_EvaluateAsync_ReturnsSuccessResult()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var entity = new TestEntity(10, "Test", true);

        // Act
        var result = await valueSpec.EvaluateAsync(entity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void CompositeSpecifications_HaveValidExpressions()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(5);
        var activeSpec = new IsActiveSpec();

        // Act & Assert
        valueSpec.ShouldHaveValidExpression();
        activeSpec.ShouldHaveValidExpression();
        (valueSpec & activeSpec).ShouldHaveValidExpression();
        (valueSpec | activeSpec).ShouldHaveValidExpression();
        (!activeSpec).ShouldHaveValidExpression();
    }

    [Fact]
    public void SpecificationFixture_ShouldMatchExactly_Works()
    {
        // Arrange
        var activeSpec = new IsActiveSpec();
        var entity1 = new TestEntity(1, "A", true);
        var entity2 = new TestEntity(2, "B", false);
        var entity3 = new TestEntity(3, "C", true);

        var fixture = SpecificationTestExtensions.CreateSpecificationFixture(entity1, entity2, entity3);

        // Act & Assert
        fixture.ShouldMatchExactly(activeSpec, entity1, entity3);
    }

    [Fact]
    public void SpecificationFixture_ShouldMatchCount_Works()
    {
        // Arrange
        var valueSpec = new ValueGreaterThanSpec(2);
        var entity1 = new TestEntity(1, "A", true);
        var entity2 = new TestEntity(5, "B", false);
        var entity3 = new TestEntity(10, "C", true);

        var fixture = SpecificationTestExtensions.CreateSpecificationFixture(entity1, entity2, entity3);

        // Act & Assert
        fixture.ShouldMatchCount(valueSpec, 2);
    }
}
