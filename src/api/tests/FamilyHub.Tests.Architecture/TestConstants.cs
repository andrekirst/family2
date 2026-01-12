namespace FamilyHub.Tests.Architecture;

/// <summary>
/// Constants for architecture tests - centralizes namespace patterns and assembly references.
/// </summary>
public static class TestConstants
{
    // Root namespace
    public const string RootNamespace = "FamilyHub";

    // Module namespaces
    public const string AuthModuleNamespace = "FamilyHub.Modules.Auth";
    public const string FamilyModuleNamespace = "FamilyHub.Modules.Family";

    // Layer suffixes (relative to module root)
    public const string DomainLayer = ".Domain";
    public const string ApplicationLayer = ".Application";
    public const string PersistenceLayer = ".Persistence";
    public const string PresentationLayer = ".Presentation";

    // SharedKernel and Infrastructure
    public const string SharedKernelNamespace = "FamilyHub.SharedKernel";
    public const string InfrastructureNamespace = "FamilyHub.Infrastructure";
    public const string ApiNamespace = "FamilyHub.Api";

    // DDD pattern suffixes
    public const string CommandsSuffix = ".Commands";
    public const string QueriesSuffix = ".Queries";
    public const string RepositoriesSuffix = ".Repositories";
    public const string EventsSuffix = ".Events";
    public const string ValueObjectsSuffix = ".ValueObjects";
    public const string AggregatesSuffix = ".Aggregates";
    public const string ExceptionsSuffix = ".Exceptions";
    public const string AbstractionsSuffix = ".Abstractions";

    // Presentation patterns
    public const string GraphQLSuffix = ".GraphQL";
    public const string MutationsSuffix = ".Mutations";
    public const string InputsSuffix = ".Inputs";
    public const string PayloadsSuffix = ".Payloads";
    public const string DataLoadersSuffix = ".DataLoaders";

    // All modules for iteration
    public static readonly string[] ModuleNamespaces =
    [
        AuthModuleNamespace,
        FamilyModuleNamespace
    ];
}
