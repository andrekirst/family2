namespace FamilyHub.Api.Common.Infrastructure.Validation;

/// <summary>
/// Category stamped on ValidationFailure.CustomState to distinguish
/// the source of a validation error at the GraphQL error filter boundary.
/// </summary>
public enum ValidatorCategory
{
    Input,
    Auth,
    Business
}
