namespace FamilyHub.Api.Common.Infrastructure.Validation;

/// <summary>
/// Marker interface for business rule validators (priority group 3).
/// These validators check async existence/uniqueness rules (entity not found, duplicates, etc.)
/// and run AFTER input and auth validators pass, with short-circuit on failure.
/// </summary>
public interface IBusinessValidator<T>;
