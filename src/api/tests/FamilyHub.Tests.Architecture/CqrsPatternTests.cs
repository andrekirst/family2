using System.Reflection;
using FluentValidation;
using MediatR;
using FamilyHub.Modules.Auth.Application.Commands.CompleteZitadelLogin;
using FamilyHub.Modules.Family.Domain.Aggregates;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Architecture tests ensuring proper CQRS (Command Query Responsibility Segregation) patterns.
/// Validates that commands, queries, handlers, and validators follow established conventions.
/// </summary>
public sealed class CqrsPatternTests
{
    private static readonly Assembly AuthModuleAssembly = typeof(CompleteZitadelLoginCommand).Assembly;
    private static readonly Assembly FamilyModuleAssembly = typeof(Family).Assembly;

    private static readonly Assembly[] AllModuleAssemblies =
    [
        AuthModuleAssembly,
        FamilyModuleAssembly
    ];

    /// <summary>
    /// Commands should implement IRequest or IRequest{TResponse} from MediatR.
    /// This ensures proper command handling infrastructure.
    /// </summary>
    [Fact]
    public void Commands_ShouldImplement_IRequest()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Commands namespaces that look like commands
        var commandTypes = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains(TestConstants.CommandsSuffix.TrimStart('.')) &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.EndsWith("Command") &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator"))
            .ToList();

        // Act & Assert
        var violatingTypes = commandTypes
            .Where(t => !ImplementsIRequest(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All command classes should implement MediatR's IRequest or IRequest<TResponse>. " +
                     $"This enables the mediator pattern for command handling. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Queries should implement IRequest or IRequest{TResponse} from MediatR.
    /// This ensures proper query handling infrastructure.
    /// </summary>
    [Fact]
    public void Queries_ShouldImplement_IRequest()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find types in Queries namespaces that look like queries
        var queryTypes = allTypes
            .Where(t => t.Namespace != null &&
                        t.Namespace.Contains(TestConstants.QueriesSuffix.TrimStart('.')) &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.EndsWith("Query") &&
                        !t.Name.Contains("Handler") &&
                        !t.Name.Contains("Validator"))
            .ToList();

        // Act & Assert
        var violatingTypes = queryTypes
            .Where(t => !ImplementsIRequest(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All query classes should implement MediatR's IRequest or IRequest<TResponse>. " +
                     $"This enables the mediator pattern for query handling. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Command and Query handlers should implement IRequestHandler.
    /// This ensures handlers can be discovered and invoked by MediatR.
    /// </summary>
    [Fact]
    public void CommandHandlers_ShouldImplement_IRequestHandler()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find handler classes
        var handlerTypes = allTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        (t.Name.EndsWith("CommandHandler") || t.Name.EndsWith("QueryHandler")))
            .ToList();

        // Act & Assert
        var violatingTypes = handlerTypes
            .Where(t => !ImplementsIRequestHandler(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All command and query handlers should implement MediatR's IRequestHandler<TRequest, TResponse>. " +
                     $"This enables automatic handler discovery and invocation. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Validators should inherit from AbstractValidator from FluentValidation.
    /// This ensures proper validation infrastructure.
    /// </summary>
    [Fact]
    public void Validators_ShouldInheritFrom_AbstractValidator()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find validator classes
        var validatorTypes = allTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.EndsWith("Validator") &&
                        !t.Name.Contains("Config"))
            .ToList();

        // Act & Assert
        var violatingTypes = validatorTypes
            .Where(t => !InheritsFromAbstractValidator(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All validator classes should inherit from FluentValidation's AbstractValidator<T>. " +
                     $"This enables automatic validation in the MediatR pipeline. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Helper method to check if a type implements IRequest or IRequest{TResponse}.
    /// </summary>
    private static bool ImplementsIRequest(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i == typeof(IRequest) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)));
    }

    /// <summary>
    /// Helper method to check if a type implements IRequestHandler.
    /// </summary>
    private static bool ImplementsIRequestHandler(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
             i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)));
    }

    /// <summary>
    /// Helper method to check if a type inherits from AbstractValidator{T}.
    /// </summary>
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
}
