using FamilyHub.Tests.Architecture.Helpers;
using FluentValidation;
using MediatR;
using NetArchTest.Rules;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Negative tests for CQRS Pattern rules.
/// These tests verify that CQRS pattern rules actually DETECT violations
/// by testing against intentionally violating types in the fixtures assembly.
/// </summary>
public sealed class CqrsPatternNegativeTests : ArchitectureTestBase
{
    private const string FixturesCqrsNamespace =
        "FamilyHub.Tests.Architecture.Fixtures.Violations.CqrsPatterns";

    /// <summary>
    /// Verifies that the rule detects commands that don't implement IRequest.
    /// </summary>
    [Fact]
    public void Commands_WhenNotImplementingIRequest_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find command types in fixtures (in Commands namespace)
        var commandTypes = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Commands") &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(commandTypes, "Command types in fixtures");

        // Act - Find commands that DON'T implement IRequest
        var violatingTypes = commandTypes
            .Where(t => !ImplementsIRequest(t))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain commands that don't implement IRequest");
        violatingTypes.Should().Contain(t => t!.Contains("BadCommand"),
            because: "BadCommand fixture should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects queries that don't implement IRequest.
    /// </summary>
    [Fact]
    public void Queries_WhenNotImplementingIRequest_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find query types in fixtures (in Queries namespace)
        var queryTypes = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Queries") &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(queryTypes, "Query types in fixtures");

        // Act - Find queries that DON'T implement IRequest
        var violatingTypes = queryTypes
            .Where(t => !ImplementsIRequest(t))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain queries that don't implement IRequest");
        violatingTypes.Should().Contain(t => t!.Contains("BadQuery"),
            because: "BadQuery fixture should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects handlers that don't implement IRequestHandler.
    /// </summary>
    [Fact]
    public void Handlers_WhenNotImplementingIRequestHandler_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find handler types in fixtures
        var handlerTypes = fixtureTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.Contains("Handler"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(handlerTypes, "Handler types in fixtures");

        // Act - Find handlers that DON'T implement IRequestHandler
        var violatingTypes = handlerTypes
            .Where(t => !ImplementsIRequestHandler(t))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain handlers that don't implement IRequestHandler");
        violatingTypes.Should().Contain(t => t!.Contains("BadCommandHandler"),
            because: "BadCommandHandler fixture should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects validators that don't inherit from AbstractValidator.
    /// </summary>
    [Fact]
    public void Validators_WhenNotInheritingAbstractValidator_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find validator types in fixtures
        var validatorTypes = fixtureTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.Contains("Validator"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(validatorTypes, "Validator types in fixtures");

        // Act - Find validators that DON'T inherit from AbstractValidator
        var violatingTypes = validatorTypes
            .Where(t => !InheritsFromAbstractValidator(t))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain validators that don't inherit from AbstractValidator");
        violatingTypes.Should().Contain(t => t!.Contains("BadCommandValidator"),
            because: "BadCommandValidator fixture should be detected as a violation");
    }

    #region Helper Methods

    private static bool ImplementsIRequest(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i == typeof(IRequest) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));
    }

    private static bool ImplementsIRequestHandler(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
             i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)));
    }

    private static bool InheritsFromAbstractValidator(Type type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }

    #endregion
}
