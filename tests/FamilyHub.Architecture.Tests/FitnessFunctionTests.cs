using System.Reflection;
using FamilyHub.Common.Application;
using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;

namespace FamilyHub.Architecture.Tests;

/// <summary>
/// Architectural fitness function tests that enforce structural invariants
/// across the codebase using NetArchTest.Rules.
/// </summary>
public class FitnessFunctionTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    private static readonly string[] FeatureModules =
    [
        "Auth", "Calendar", "Dashboard", "EventChain", "Family",
        "FileManagement", "GoogleIntegration", "Messaging", "Photos", "School", "Search"
    ];

    // ── Handler Sealing ────────────────────────────────────────────

    [Fact]
    public void All_command_handlers_should_be_sealed()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        var failingTypes = result.FailingTypeNames ?? [];

        failingTypes.Should().BeEmpty(
            "all command handlers must be sealed to prevent unintended inheritance. " +
            "Unsealed: {0}", string.Join(", ", failingTypes));
    }

    [Fact]
    public void All_query_handlers_should_be_sealed()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        var failingTypes = result.FailingTypeNames ?? [];

        failingTypes.Should().BeEmpty(
            "all query handlers must be sealed to prevent unintended inheritance. " +
            "Unsealed: {0}", string.Join(", ", failingTypes));
    }

    // ── Handler Interface Compliance ───────────────────────────────

    [Fact]
    public void All_classes_named_CommandHandler_should_implement_ICommandHandler()
    {
        var handlerTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .ToList();

        var violations = handlerTypes
            .Where(t => !t.GetInterfaces().Any(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            "all classes ending with 'CommandHandler' must implement ICommandHandler<,>. " +
            "Violations: {0}", string.Join(", ", violations));
    }

    [Fact]
    public void All_classes_named_QueryHandler_should_implement_IQueryHandler()
    {
        var handlerTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.Name.EndsWith("QueryHandler"))
            .ToList();

        var violations = handlerTypes
            .Where(t => !t.GetInterfaces().Any(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            "all classes ending with 'QueryHandler' must implement IQueryHandler<,>. " +
            "Violations: {0}", string.Join(", ", violations));
    }

    // ── Validator Compliance ───────────────────────────────────────

    [Fact]
    public void All_validators_should_inherit_from_AbstractValidator()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Validator")
            .And()
            .AreNotAbstract()
            .And()
            .AreNotInterfaces()
            .Should()
            .Inherit(typeof(AbstractValidator<>))
            .GetResult();

        var failingTypes = result.FailingTypeNames ?? [];

        failingTypes.Should().BeEmpty(
            "all classes ending with 'Validator' must inherit from AbstractValidator<T>. " +
            "Violations: {0}", string.Join(", ", failingTypes));
    }

    // ── Handler Naming Suffix ──────────────────────────────────────

    [Fact]
    public void All_ICommandHandler_implementations_should_end_with_Handler()
    {
        var handlerTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)))
            .ToList();

        var violations = handlerTypes
            .Where(t => !t.Name.EndsWith("Handler"))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            "all ICommandHandler implementations must have a name ending with 'Handler'. " +
            "Violations: {0}", string.Join(", ", violations));
    }

    [Fact]
    public void All_IQueryHandler_implementations_should_end_with_Handler()
    {
        var handlerTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            .ToList();

        var violations = handlerTypes
            .Where(t => !t.Name.EndsWith("Handler"))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            "all IQueryHandler implementations must have a name ending with 'Handler'. " +
            "Violations: {0}", string.Join(", ", violations));
    }

    [Fact]
    public void All_AbstractValidator_implementations_should_end_with_Validator()
    {
        var validatorTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.BaseType is { IsGenericType: true }
                && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .ToList();

        var violations = validatorTypes
            .Where(t => !t.Name.EndsWith("Validator"))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            "all AbstractValidator<T> implementations must have a name ending with 'Validator'. " +
            "Violations: {0}", string.Join(", ", violations));
    }

    // ── Domain Layer Isolation ─────────────────────────────────────

    [Fact]
    public void Domain_entities_should_not_depend_on_Infrastructure()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespaceContaining(".Domain.")
            .ShouldNot()
            .HaveDependencyOn("FamilyHub.Api.Features.*.Infrastructure")
            .GetResult();

        // NetArchTest wildcard matching may not cover all patterns,
        // so also verify with explicit per-module checks.
        var domainTypes = ApiAssembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.Contains(".Domain."))
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToList();

        var violations = new List<string>();

        foreach (var type in domainTypes)
        {
            var referencedNamespaces = type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(f => f.FieldType.Namespace)
                .Concat(type
                    .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Select(p => p.PropertyType.Namespace))
                .Concat(type
                    .GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Select(p => p.ParameterType.Namespace))
                .Where(ns => ns is not null)
                .Distinct();

            foreach (var ns in referencedNamespaces)
            {
                if (ns!.Contains(".Infrastructure"))
                {
                    violations.Add($"{type.FullName} depends on infrastructure namespace '{ns}'");
                }
            }
        }

        violations.Should().BeEmpty(
            "domain entities must not depend on infrastructure. " +
            "Violations: {0}", string.Join("; ", violations));
    }

    // ── Cross-Module Isolation ─────────────────────────────────────

    /// <summary>
    /// Known cross-module domain references that exist as technical debt.
    /// These are allowed pairs (source module -> target module) that were present
    /// before the fitness function was introduced. No NEW pairs should be added.
    /// Track removal progress via issue backlog.
    /// </summary>
    private static readonly HashSet<(string Source, string Target)> KnownCrossModuleViolations =
    [
        ("Auth", "Family"),
        ("Family", "Auth"),
        ("FileManagement", "Family"),
        ("FileManagement", "Auth"),
        ("GoogleIntegration", "Auth"),
        ("Messaging", "Auth"),
        ("Messaging", "Family"),
        ("Messaging", "FileManagement"),
        ("Photos", "FileManagement"),
        ("School", "Family"),
    ];

    [Fact]
    public void No_module_should_reference_another_modules_Domain_namespace()
    {
        var violations = new List<string>();

        foreach (var module in FeatureModules)
        {
            var moduleNamespace = $"FamilyHub.Api.Features.{module}";

            var moduleTypes = ApiAssembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && t.Namespace.StartsWith(moduleNamespace, StringComparison.Ordinal))
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .ToList();

            foreach (var type in moduleTypes)
            {
                var referencedNamespaces = GetReferencedNamespaces(type);

                foreach (var otherModule in FeatureModules)
                {
                    if (otherModule == module) continue;

                    // Skip known technical debt pairs
                    if (KnownCrossModuleViolations.Contains((module, otherModule)))
                        continue;

                    var forbiddenDomainNs = $"FamilyHub.Api.Features.{otherModule}.Domain";

                    foreach (var ns in referencedNamespaces)
                    {
                        if (ns.StartsWith(forbiddenDomainNs, StringComparison.Ordinal))
                        {
                            violations.Add(
                                $"{type.FullName} ({module}) references domain namespace '{ns}' from module '{otherModule}'");
                        }
                    }
                }
            }
        }

        violations.Should().BeEmpty(
            "modules must not directly reference another module's Domain namespace. " +
            "Use shared contracts or domain events for cross-module communication. " +
            "Known exceptions are tracked in KnownCrossModuleViolations. " +
            "New violations: {0}", string.Join("; ", violations));
    }

    // ── Command-Validator Pairing ──────────────────────────────────

    /// <summary>
    /// Commands that currently lack validators. These are tracked as technical debt.
    /// No NEW commands should be added without a validator.
    /// </summary>
    private static readonly HashSet<string> KnownCommandsWithoutValidator =
    [
        // FileManagement module - bulk commands added before validator requirement
        "FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateSecureNote.UpdateSecureNoteCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateOrganizationRule.UpdateOrganizationRuleCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.UntagFile.UntagFileCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleOrganizationRule.ToggleOrganizationRuleCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleFavorite.ToggleFavoriteCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.TagFile.TagFileCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.SetPermission.SetPermissionCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.SaveSearch.SaveSearchCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.RevokeShareLink.RevokeShareLinkCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion.RestoreFileVersionCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.ReorderOrganizationRules.ReorderOrganizationRulesCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.RenameAlbum.RenameAlbumCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.RemovePermission.RemovePermissionCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.RemoveFileFromAlbum.RemoveFileFromAlbumCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.ProcessInboxFiles.ProcessInboxFilesCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.GenerateThumbnails.GenerateThumbnailsCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.DisconnectExternalStorage.DisconnectExternalStorageCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteTag.DeleteTagCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSecureNote.DeleteSecureNoteCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteSavedSearch.DeleteSavedSearchCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteOrganizationRule.DeleteOrganizationRuleCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteAlbum.DeleteAlbumCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.CreateZipJob.CreateZipJobCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink.CreateShareLinkCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.CreateSecureNote.CreateSecureNoteCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.CreateOrganizationRule.CreateOrganizationRuleCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion.CreateFileVersionCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.ConnectExternalStorage.ConnectExternalStorageCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.AddFileToAlbum.AddFileToAlbumCommand",
        "FamilyHub.Api.Features.FileManagement.Application.Commands.AccessShareLink.AccessShareLinkCommand",
        // EventChain module - commands added before validator requirement
        "FamilyHub.Api.Features.EventChain.Application.Commands.UpdateChainDefinition.UpdateChainDefinitionCommand",
        "FamilyHub.Api.Features.EventChain.Application.Commands.EnableChainDefinition.EnableChainDefinitionCommand",
        "FamilyHub.Api.Features.EventChain.Application.Commands.DisableChainDefinition.DisableChainDefinitionCommand",
    ];

    [Fact]
    public void All_commands_should_have_at_least_one_validator()
    {
        var commandTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)))
            .ToList();

        var validatorTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.BaseType is { IsGenericType: true }
                && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .ToList();

        var validatedCommandTypes = validatorTypes
            .Select(v => v.BaseType!.GetGenericArguments()[0])
            .Distinct()
            .ToHashSet();

        var commandsWithoutValidator = commandTypes
            .Where(c => !validatedCommandTypes.Contains(c))
            .Where(c => !KnownCommandsWithoutValidator.Contains(c.FullName!))
            .Select(c => c.FullName!)
            .ToList();

        commandsWithoutValidator.Should().BeEmpty(
            "every NEW command must have at least one validator (CommandValidator, BusinessValidator, or AuthValidator). " +
            "Known exceptions are tracked in KnownCommandsWithoutValidator. " +
            "Missing validators for: {0}", string.Join(", ", commandsWithoutValidator));
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static IEnumerable<string> GetReferencedNamespaces(Type type)
    {
        var namespaces = new HashSet<string>();

        void AddNamespace(Type? t)
        {
            if (t?.Namespace is not null)
                namespaces.Add(t.Namespace);

            if (t is { IsGenericType: true })
            {
                foreach (var arg in t.GetGenericArguments())
                    AddNamespace(arg);
            }
        }

        // Fields
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            AddNamespace(field.FieldType);

        // Properties
        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            AddNamespace(prop.PropertyType);

        // Constructor parameters
        foreach (var ctor in type.GetConstructors())
        foreach (var param in ctor.GetParameters())
            AddNamespace(param.ParameterType);

        // Method parameters and return types
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            AddNamespace(method.ReturnType);
            foreach (var param in method.GetParameters())
                AddNamespace(param.ParameterType);
        }

        // Base type
        AddNamespace(type.BaseType);

        // Interfaces
        foreach (var iface in type.GetInterfaces())
            AddNamespace(iface);

        return namespaces;
    }
}
