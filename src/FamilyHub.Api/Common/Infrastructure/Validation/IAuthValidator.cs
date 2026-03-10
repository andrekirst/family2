namespace FamilyHub.Api.Common.Infrastructure.Validation;

/// <summary>
/// Marker interface for authorization validators (priority group 2).
/// These validators check async permission/role rules (CanInvite, CanManageStudents, etc.)
/// and run AFTER input validators pass, with short-circuit on failure.
/// </summary>
public interface IAuthValidator<T>;
