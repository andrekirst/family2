using FamilyDomain = FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using FamilyHub.Modules.Auth.Application.Abstractions;
using MediatR;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Fluent builder for resolving services from a DI scope in integration tests.
/// Supports flexible service combinations with compile-time type safety.
/// </summary>
/// <example>
/// var resolver = TestServiceResolver.For(scope)
///     .WithMediator()
///     .WithUserRepository()
///     .WithFamilyRepository()
///     .WithUnitOfWork();
///
/// var mediator = resolver.Get&lt;IMediator&gt;();
/// var userRepo = resolver.Get&lt;IUserRepository&gt;();
/// </example>
public sealed class TestServiceResolver
{
    private readonly IServiceScope _scope;
    private IMediator? _mediator;
    private IUserRepository? _userRepository;
    private IFamilyRepository? _familyRepository;
    private IUnitOfWork? _unitOfWork;
    private ICurrentUserService? _currentUserService;
    private IHttpClientFactory? _httpClientFactory;

    private TestServiceResolver(IServiceScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        _scope = scope;
    }

    /// <summary>
    /// Creates a new service resolver for the given scope.
    /// </summary>
    public static TestServiceResolver For(IServiceScope scope) => new(scope);

    /// <summary>
    /// Resolves and includes IMediator.
    /// </summary>
    public TestServiceResolver WithMediator()
    {
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
        return this;
    }

    /// <summary>
    /// Resolves and includes IUserRepository.
    /// </summary>
    public TestServiceResolver WithUserRepository()
    {
        _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        return this;
    }

    /// <summary>
    /// Resolves and includes IFamilyRepository.
    /// </summary>
    public TestServiceResolver WithFamilyRepository()
    {
        _familyRepository = _scope.ServiceProvider.GetRequiredService<IFamilyRepository>();
        return this;
    }

    /// <summary>
    /// Resolves and includes IUnitOfWork.
    /// </summary>
    public TestServiceResolver WithUnitOfWork()
    {
        _unitOfWork = _scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        return this;
    }

    /// <summary>
    /// Resolves and includes ICurrentUserService (typically mocked).
    /// </summary>
    public TestServiceResolver WithCurrentUserService()
    {
        _currentUserService = _scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        return this;
    }

    /// <summary>
    /// Resolves and includes IHttpClientFactory (typically mocked).
    /// </summary>
    public TestServiceResolver WithHttpClientFactory()
    {
        _httpClientFactory = _scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        return this;
    }

    /// <summary>
    /// Gets a specific service by type. Throws if not resolved via With*().
    /// </summary>
    public T Get<T>() where T : class
    {
        return typeof(T).Name switch
        {
            nameof(IMediator) => (_mediator as T) ?? throw NotResolved<T>(),
            nameof(IUserRepository) => (_userRepository as T) ?? throw NotResolved<T>(),
            nameof(IFamilyRepository) => (_familyRepository as T) ?? throw NotResolved<T>(),
            nameof(IUnitOfWork) => (_unitOfWork as T) ?? throw NotResolved<T>(),
            nameof(ICurrentUserService) => (_currentUserService as T) ?? throw NotResolved<T>(),
            nameof(IHttpClientFactory) => (_httpClientFactory as T) ?? throw NotResolved<T>(),
            _ => throw new InvalidOperationException($"Service {typeof(T).Name} not supported by TestServiceResolver.")
        };
    }

    private static InvalidOperationException NotResolved<T>()
        => new($"Service {typeof(T).Name} not resolved. Call With{typeof(T).Name}() first.");
}
