using System.Reflection;
using FamilyHub.Modules.Auth.Application.Commands.CreateFamily;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.SharedKernel.Application.CQRS;
using FluentValidation;

namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Architecture tests ensuring proper CQRS (Command Query Responsibility Segregation) patterns.
/// Validates that commands, queries, handlers, and validators follow established conventions.
/// </summary>
public sealed class CqrsPatternTests
{
    private static readonly Assembly AuthModuleAssembly = typeof(CreateFamilyCommand).Assembly;
    private static readonly Assembly FamilyModuleAssembly = typeof(Family).Assembly;

    private static readonly Assembly[] AllModuleAssemblies =
    [
        AuthModuleAssembly,
        FamilyModuleAssembly
    ];

    /// <summary>
    /// Commands should implement ICommand or ICommand{TResponse}.
    /// This ensures proper command handling infrastructure and type-safe CQRS patterns.
    /// </summary>
    [Fact]
    public void Commands_ShouldImplement_ICommand()
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
            .Where(t => !ImplementsICommand(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All command classes should implement ICommand or ICommand<TResponse>. " +
                     $"This enables type-safe CQRS pattern enforcement. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Queries should implement IQuery or IQuery{TResponse}.
    /// This ensures proper query handling infrastructure and type-safe CQRS patterns.
    /// </summary>
    [Fact]
    public void Queries_ShouldImplement_IQuery()
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
            .Where(t => !ImplementsIQuery(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All query classes should implement IQuery or IQuery<TResponse>. " +
                     $"This enables type-safe CQRS pattern enforcement. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Command handlers should implement ICommandHandler.
    /// This ensures handlers can be discovered and invoked by MediatR.
    /// </summary>
    [Fact]
    public void CommandHandlers_ShouldImplement_ICommandHandler()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find command handler classes
        var handlerTypes = allTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.EndsWith("CommandHandler"))
            .ToList();

        // Act & Assert
        var violatingTypes = handlerTypes
            .Where(t => !ImplementsICommandHandler(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All command handlers should implement ICommandHandler<TCommand, TResponse>. " +
                     $"This enables type-safe CQRS pattern enforcement. " +
                     $"Violating types: {string.Join(", ", violatingTypes)}");
    }

    /// <summary>
    /// Query handlers should implement IQueryHandler.
    /// This ensures handlers can be discovered and invoked by MediatR.
    /// </summary>
    [Fact]
    public void QueryHandlers_ShouldImplement_IQueryHandler()
    {
        // Arrange
        var allTypes = AllModuleAssemblies
            .SelectMany(a => Types.InAssembly(a).GetTypes())
            .ToList();

        // Find query handler classes
        var handlerTypes = allTypes
            .Where(t => t.IsClass &&
                        !t.IsAbstract &&
                        t.Name.EndsWith("QueryHandler"))
            .ToList();

        // Act & Assert
        var violatingTypes = handlerTypes
            .Where(t => !ImplementsIQueryHandler(t))
            .Select(t => t.FullName)
            .ToList();

        violatingTypes.Should().BeEmpty(
            because: $"All query handlers should implement IQueryHandler<TQuery, TResponse>. " +
                     $"This enables type-safe CQRS pattern enforcement. " +
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
    /// Helper method to check if a type implements ICommand or ICommand{TResponse}.
    /// </summary>
    private static bool ImplementsICommand(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i == typeof(ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));
    }

    /// <summary>
    /// Helper method to check if a type implements IQuery or IQuery{TResponse}.
    /// </summary>
    private static bool ImplementsIQuery(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i == typeof(IQuery) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)));
    }

    /// <summary>
    /// Helper method to check if a type implements ICommandHandler.
    /// </summary>
    private static bool ImplementsICommandHandler(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
             i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)));
    }

    /// <summary>
    /// Helper method to check if a type implements IQueryHandler.
    /// </summary>
    private static bool ImplementsIQueryHandler(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) ||
             i.GetGenericTypeDefinition() == typeof(IQueryHandler<>)));
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
