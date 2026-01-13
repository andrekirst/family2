using FamilyHub.Tests.Architecture.Helpers;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Negative tests for Naming Convention rules.
/// These tests verify that naming convention rules actually DETECT violations
/// by testing against intentionally violating types in the fixtures assembly.
/// </summary>
public sealed class NamingConventionNegativeTests : ArchitectureTestBase
{
    private const string FixturesNamingNamespace =
        "FamilyHub.Tests.Architecture.Fixtures.Violations.NamingConventions";

    /// <summary>
    /// Verifies that the rule detects interfaces without 'I' prefix.
    /// </summary>
    [Fact]
    public void Interfaces_WhenMissingIPrefix_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find interfaces in naming conventions fixtures
        var interfaces = fixtureTypes
            .Where(t => t.IsInterface &&
                        t.Namespace != null &&
                        t.Namespace.Contains("NamingConventions"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(interfaces, "Interfaces in naming conventions fixtures");

        // Act - Find interfaces that DON'T start with 'I'
        var violatingTypes = interfaces
            .Where(t => !t.Name.StartsWith("I") || t.Name.Length < 2 || !char.IsUpper(t.Name[1]))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain interfaces without 'I' prefix");
        violatingTypes.Should().Contain(t => t!.Contains("BadlyNamedService"),
            because: "BadlyNamedService interface should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects commands without 'Command' suffix.
    /// </summary>
    [Fact]
    public void Commands_WhenMissingCommandSuffix_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find command-like types in fixtures (in Commands namespace, implementing IRequest)
        var commandNamespace = $"{FixturesNamingNamespace}.Application.Commands";
        var potentialCommands = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Commands") &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(potentialCommands, "Command types in naming conventions fixtures");

        // Act - Find commands that DON'T end with 'Command'
        var violatingTypes = potentialCommands
            .Where(t => !t.Name.EndsWith("Command"))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain commands without 'Command' suffix");
        violatingTypes.Should().Contain(t => t!.Contains("CreateUser"),
            because: "CreateUser (without Command suffix) should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects queries without 'Query' suffix.
    /// </summary>
    [Fact]
    public void Queries_WhenMissingQuerySuffix_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find query-like types in fixtures (in Queries namespace)
        var potentialQueries = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Queries") &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(potentialQueries, "Query types in naming conventions fixtures");

        // Act - Find queries that DON'T end with 'Query'
        var violatingTypes = potentialQueries
            .Where(t => !t.Name.EndsWith("Query"))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain queries without 'Query' suffix");
        violatingTypes.Should().Contain(t => t!.Contains("GetUser"),
            because: "GetUser (without Query suffix) should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects GraphQL inputs without 'Input' suffix.
    /// </summary>
    [Fact]
    public void GraphQLInputs_WhenMissingInputSuffix_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find input-like types in fixtures (in Inputs namespace)
        var inputTypes = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Inputs") &&
                        t.IsClass &&
                        !t.IsAbstract)
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(inputTypes, "Input types in naming conventions fixtures");

        // Act - Find inputs that DON'T end with 'Input'
        var violatingTypes = inputTypes
            .Where(t => !t.Name.EndsWith("Input"))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain inputs without 'Input' suffix");
        violatingTypes.Should().Contain(t => t!.Contains("CreateUserData"),
            because: "CreateUserData (without Input suffix) should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects domain events without 'Event' suffix.
    /// </summary>
    [Fact]
    public void DomainEvents_WhenMissingEventSuffix_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find event-like types in fixtures (in Events namespace)
        var eventTypes = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Events") &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler"))
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(eventTypes, "Event types in naming conventions fixtures");

        // Act - Find events that DON'T end with 'Event'
        var violatingTypes = eventTypes
            .Where(t => !t.Name.EndsWith("Event"))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain events without 'Event' suffix");
        violatingTypes.Should().Contain(t => t!.Contains("UserCreated"),
            because: "UserCreated (without Event suffix) should be detected as a violation");
    }

    /// <summary>
    /// Verifies that the rule detects GraphQL payloads without 'Payload' suffix.
    /// </summary>
    [Fact]
    public void GraphQLPayloads_WhenMissingPayloadSuffix_ShouldBeDetected()
    {
        // Arrange
        var fixtureTypes = Types.InAssembly(ViolationFixturesAssembly).GetTypes();

        // Find payload-like types in fixtures (in Payloads namespace)
        var payloadTypes = fixtureTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains("Payloads") &&
                        t.IsClass &&
                        !t.IsAbstract)
            .ToList();

        // Guard
        GuardAgainstEmptyTypes(payloadTypes, "Payload types in naming conventions fixtures");

        // Act - Find payloads that DON'T end with 'Payload'
        var violatingTypes = payloadTypes
            .Where(t => !t.Name.EndsWith("Payload"))
            .Select(t => t.FullName)
            .ToList();

        // Assert - We EXPECT violations
        violatingTypes.Should().NotBeEmpty(
            because: "Negative test fixtures should contain payloads without 'Payload' suffix");
        violatingTypes.Should().Contain(t => t!.Contains("CreateUserResult"),
            because: "CreateUserResult (without Payload suffix) should be detected as a violation");
    }
}
