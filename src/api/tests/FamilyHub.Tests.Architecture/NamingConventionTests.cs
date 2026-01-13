using System.Reflection;
using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Architecture tests ensuring consistent naming conventions across the codebase.
/// Proper naming conventions improve code readability and maintainability.
/// </summary>
public sealed class NamingConventionTests
{
    private static readonly Assembly AuthModuleAssembly = typeof(CompleteZitadelLoginCommand).Assembly;
    private static readonly Assembly FamilyModuleAssembly = typeof(Family).Assembly;
    private static readonly Assembly SharedKernelAssembly = typeof(AggregateRoot<>).Assembly;

    private static readonly Assembly[] AllAssemblies =
    [
        AuthModuleAssembly,
        FamilyModuleAssembly,
        SharedKernelAssembly
    ];

    /// <summary>
    /// Interfaces should start with 'I' prefix.
    /// This is a standard .NET naming convention.
    /// </summary>
    [Fact]
    public void Interfaces_ShouldStartWith_I()
    {
        // Arrange
        var allTypes = AllAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find all interfaces
        var interfaces = allTypes
            .Where(t => t.IsInterface &&
                        t.Namespace != null &&
                        !t.Namespace.Contains("obj") &&
                        !t.Name.Contains("<")) // Exclude compiler-generated
            .ToList();

        // Act & Assert
        var violatingTypes = interfaces
            .Where(t => !t.Name.StartsWith("I") || t.Name.Length < 2 || !char.IsUpper(t.Name[1]))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All interfaces should follow the .NET naming convention of starting with 'I' prefix. " +
                     $"Example: IUserRepository, IFamilyService. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Command classes should end with 'Command' suffix.
    /// This makes it clear they represent write operations.
    /// </summary>
    [Fact]
    public void Commands_ShouldEndWith_Command()
    {
        // Arrange
        var allTypes = AllAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Commands namespaces that appear to be command classes
        var potentialCommands = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains(TestConstants.CommandsSuffix.TrimStart('.')) &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator") &&
                        !t.Name.Contains("Result") &&
                        !t.Name.Contains("Response") &&
                        !t.Name.Contains("Behavior") &&
                        !t.Name.Contains("Exception") &&
                        !t.Name.Contains("<") && // Exclude compiler-generated
                        !t.Name.EndsWith("Command"))
            .ToList();

        // Act & Assert
        potentialCommands.Should().BeEmpty(
            because: $"All command classes in .Commands namespace should end with 'Command' suffix. " +
                     $"This clearly identifies them as CQRS commands representing write operations. " +
                     $"Potential violations: {string.Join(", ", potentialCommands.Select(t => t.FullName))}");
    }

    /// <summary>
    /// CQRS Query classes (implementing IRequest) should end with 'Query' suffix.
    /// This makes it clear they represent read operations.
    /// Note: GraphQL query extension classes and DTOs in Queries namespace are excluded.
    /// </summary>
    [Fact]
    public void Queries_ShouldEndWith_Query()
    {
        // Arrange
        var allTypes = AllAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Application.Queries namespaces that appear to be CQRS query classes
        // Exclude: Handlers, Validators, Results, DTOs, GraphQL extensions
        var potentialQueries = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains($"{TestConstants.ApplicationLayer}.Queries".TrimStart('.')) &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator") &&
                        !t.Name.Contains("Result") &&
                        !t.Name.Contains("Response") &&
                        !t.Name.EndsWith("Dto") &&
                        !t.Name.Contains("<") && // Exclude compiler-generated
                        !t.Name.EndsWith("Query"))
            .ToList();

        // Act & Assert
        potentialQueries.Should().BeEmpty(
            because: $"All CQRS query classes in .Application.Queries namespace should end with 'Query' suffix. " +
                     $"This clearly identifies them as CQRS queries representing read operations. " +
                     $"Potential violations: {string.Join(", ", potentialQueries.Select(t => t.FullName))}");
    }

    /// <summary>
    /// GraphQL Input types should end with 'Input' suffix.
    /// This follows Hot Chocolate conventions and clearly identifies DTO types.
    /// </summary>
    [Fact]
    public void GraphQLInputs_ShouldEndWith_Input()
    {
        // Arrange
        var allTypes = AllAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Inputs namespaces
        var inputTypes = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains(TestConstants.InputsSuffix.TrimStart('.')) &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Validator") &&
                        !t.Name.Contains("<")) // Exclude compiler-generated
            .ToList();

        // Act & Assert
        var violatingTypes = inputTypes
            .Where(t => !t.Name.EndsWith("Input"))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All GraphQL input types in .Inputs namespace should end with 'Input' suffix. " +
                     $"This follows Hot Chocolate conventions and clearly identifies GraphQL input DTOs. " +
                     $"Example: CreateFamilyInput, UpdateUserInput. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Domain events should end with 'Event' suffix.
    /// This clearly identifies them as domain events.
    /// </summary>
    [Fact]
    public void DomainEvents_ShouldEndWith_Event()
    {
        // Arrange
        var allTypes = AllAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Events namespaces
        var eventTypes = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains($"{TestConstants.DomainLayer}.Events".TrimStart('.')) &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("<")) // Exclude compiler-generated
            .ToList();

        // Act & Assert
        var violatingTypes = eventTypes
            .Where(t => !t.Name.EndsWith("Event"))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All domain event classes should end with 'Event' suffix. " +
                     $"This clearly identifies them as domain events for the event-driven architecture. " +
                     $"Example: FamilyCreatedEvent, UserRegisteredEvent. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// GraphQL mutation return types should end with 'Payload' suffix.
    /// This follows Hot Chocolate conventions for mutation results.
    /// Note: Helper types like Error classes and DTOs in Payloads namespace are excluded.
    /// </summary>
    [Fact]
    public void GraphQLPayloads_ShouldEndWith_Payload()
    {
        // Arrange
        var allTypes = AllAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Payloads namespaces that are the actual payload types
        // Exclude: Error types, DTOs, and other helper classes
        var payloadTypes = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.EndsWith(TestConstants.PayloadsSuffix.TrimStart('.')) &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.Name.Contains("Error") &&
                        !t.Name.EndsWith("Dto") &&
                        !t.Name.Contains("<")) // Exclude compiler-generated
            .ToList();

        // Act & Assert
        var violatingTypes = payloadTypes
            .Where(t => !t.Name.EndsWith("Payload"))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"GraphQL payload types in .Payloads namespace should end with 'Payload' suffix. " +
                     $"This follows Hot Chocolate conventions for mutation return types. " +
                     $"Example: CreateFamilyPayload, LoginPayload. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }
}
