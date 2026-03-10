using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Api.Common.Infrastructure.Validation;

/// <summary>
/// Extension methods for registering validator groups (Input, Auth, Business)
/// alongside the standard FluentValidation assembly scanning.
/// </summary>
public static class ValidatorRegistrationExtensions
{
    /// <summary>
    /// Scans the assembly for validators implementing marker interfaces
    /// (IInputValidator, IAuthValidator, IBusinessValidator) and registers them.
    /// Call after AddValidatorsFromAssemblyContaining to ensure all validators are registered.
    /// The marker interfaces are used at runtime by ValidationBehavior to partition validators into groups.
    /// No additional DI registration is needed — validators are already registered by FluentValidation's
    /// assembly scan. This method exists as a hook for future registration needs.
    /// </summary>
    public static IServiceCollection AddValidatorGroups(this IServiceCollection services)
    {
        // FluentValidation's AddValidatorsFromAssemblyContaining already registers all
        // AbstractValidator<T> implementations. The marker interfaces (IInputValidator<T>,
        // IAuthValidator<T>, IBusinessValidator<T>) are checked at runtime via `is` checks
        // in ValidationBehavior, so no additional DI registration is required.
        //
        // This method serves as an explicit opt-in point in Program.cs and a future
        // extension point if we need marker-based DI registrations later.
        return services;
    }
}
