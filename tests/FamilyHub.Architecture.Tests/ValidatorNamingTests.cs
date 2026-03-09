using System.Reflection;
using FluentAssertions;
using FluentValidation;

namespace FamilyHub.Architecture.Tests;

/// <summary>
/// Ensures all FluentValidation validators follow naming conventions:
///   - *CommandValidator (IInputValidator)
///   - *QueryValidator (read-only query validators)
///   - *BusinessValidator (IBusinessValidator)
///   - *AuthValidator (IAuthValidator)
/// </summary>
public class ValidatorNamingTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void All_validators_should_follow_naming_convention()
    {
        var validatorTypes = ApiAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.BaseType is { IsGenericType: true }
                && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .ToList();

        var violations = new List<string>();

        foreach (var type in validatorTypes)
        {
            var name = type.Name;
            var isValid = name.EndsWith("CommandValidator")
                || name.EndsWith("QueryValidator")
                || name.EndsWith("BusinessValidator")
                || name.EndsWith("AuthValidator");

            if (!isValid)
            {
                violations.Add($"{type.FullName} does not follow naming convention (*CommandValidator, *QueryValidator, *BusinessValidator, *AuthValidator)");
            }
        }

        violations.Should().BeEmpty(
            "all validators should follow naming conventions: *CommandValidator, *QueryValidator, *BusinessValidator, or *AuthValidator");
    }
}
