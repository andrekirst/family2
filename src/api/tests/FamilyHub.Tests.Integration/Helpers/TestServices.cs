using Microsoft.Extensions.DependencyInjection;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.SharedKernel.Interfaces;
using MediatR;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Static helper class providing predefined service resolution patterns
/// and generic resolution methods.
/// </summary>
public static class TestServices
{
    /// <summary>
    /// Resolves the most common service set for command/query integration tests:
    /// IMediator, IUserRepository, IFamilyRepository, IUnitOfWork
    /// </summary>
    /// <example>
    /// var (mediator, userRepo, familyRepo, unitOfWork) = TestServices.ResolveCommandServices(scope);
    /// </example>
    public static (IMediator mediator, IUserRepository userRepo,
                   IFamilyRepository familyRepo, IUnitOfWork unitOfWork)
        ResolveCommandServices(IServiceScope scope)
    {
        return (
            scope.ServiceProvider.GetRequiredService<IMediator>(),
            scope.ServiceProvider.GetRequiredService<IUserRepository>(),
            scope.ServiceProvider.GetRequiredService<IFamilyRepository>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
        );
    }

    /// <summary>
    /// Resolves common repository services for GraphQL mutation tests:
    /// IUserRepository, IUnitOfWork
    /// </summary>
    /// <example>
    /// var (userRepo, unitOfWork) = TestServices.ResolveRepositoryServices(scope);
    /// </example>
    public static (IUserRepository userRepo, IFamilyRepository familyRepo, IUnitOfWork unitOfWork)
        ResolveRepositoryServices(IServiceScope scope)
    {
        return (
            scope.ServiceProvider.GetRequiredService<IUserRepository>(),
            scope.ServiceProvider.GetRequiredService<IFamilyRepository>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
        );
    }

    /// <summary>
    /// Resolves services for OAuth flow tests:
    /// IMediator, IUserRepository
    /// </summary>
    /// <example>
    /// var (mediator, userRepo) = TestServices.ResolveOAuthServices(scope);
    /// </example>
    public static (IMediator mediator, IUserRepository userRepo)
        ResolveOAuthServices(IServiceScope scope)
    {
        return (
            scope.ServiceProvider.GetRequiredService<IMediator>(),
            scope.ServiceProvider.GetRequiredService<IUserRepository>()
        );
    }

    /// <summary>
    /// Generic service resolution for custom combinations (1-4 services).
    /// </summary>
    public static T Resolve<T>(IServiceScope scope) where T : class
        => scope.ServiceProvider.GetRequiredService<T>();

    public static (T1, T2) Resolve<T1, T2>(IServiceScope scope)
        where T1 : class
        where T2 : class
    {
        return (
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>()
        );
    }

    public static (T1, T2, T3) Resolve<T1, T2, T3>(IServiceScope scope)
        where T1 : class
        where T2 : class
        where T3 : class
    {
        return (
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>(),
            scope.ServiceProvider.GetRequiredService<T3>()
        );
    }

    public static (T1, T2, T3, T4) Resolve<T1, T2, T3, T4>(IServiceScope scope)
        where T1 : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        return (
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>(),
            scope.ServiceProvider.GetRequiredService<T3>(),
            scope.ServiceProvider.GetRequiredService<T4>()
        );
    }
}
