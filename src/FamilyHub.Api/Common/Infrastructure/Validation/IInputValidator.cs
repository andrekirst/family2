namespace FamilyHub.Api.Common.Infrastructure.Validation;

/// <summary>
/// Marker interface for input/format validators (priority group 1).
/// These validators check synchronous format and schema rules (required fields, email format, etc.)
/// and run BEFORE auth and business validators with short-circuit on failure.
/// </summary>
public interface IInputValidator<T>;
