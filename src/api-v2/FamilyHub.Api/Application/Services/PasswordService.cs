using FamilyHub.Api.Domain.ValueObjects;

namespace FamilyHub.Api.Application.Services;

public interface IPasswordService
{
    PasswordHash HashPassword(string password);
    bool VerifyPassword(string password, PasswordHash hash);
    (bool IsValid, string? Error) ValidatePasswordStrength(string password);
}

public sealed class PasswordService : IPasswordService
{
    public PasswordHash HashPassword(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        return PasswordHash.From(hash);
    }

    public bool VerifyPassword(string password, PasswordHash hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash.Value);
    }

    public (bool IsValid, string? Error) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password cannot be empty");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters long");

        if (password.Length > 128)
            return (false, "Password cannot exceed 128 characters");

        if (!password.Any(char.IsUpper))
            return (false, "Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            return (false, "Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            return (false, "Password must contain at least one digit");

        return (true, null);
    }
}
