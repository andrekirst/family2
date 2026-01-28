using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace FamilyHub.Modules.Auth.Infrastructure.Services;

/// <summary>
/// Password service using ASP.NET Core Identity's PasswordHasher (Argon2id in .NET 9+).
/// </summary>
public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly PasswordPolicyOptions _policyOptions;

    /// <summary>
    /// Initializes a new instance of the PasswordService.
    /// </summary>
    /// <param name="policyOptions">Password policy configuration options.</param>
    public PasswordService(IOptions<PasswordPolicyOptions> policyOptions)
    {
        _passwordHasher = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions
        {
            // Use the default algorithm (PBKDF2 in .NET 8, Argon2id in .NET 9+)
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3
        }));
        _policyOptions = policyOptions.Value;
    }

    /// <inheritdoc />
    public PasswordHash HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // PasswordHasher uses a pseudo-user since it doesn't need user data for hashing
        var hash = _passwordHasher.HashPassword(null!, password);
        return PasswordHash.FromHash(hash);
    }

    /// <inheritdoc />
    public bool VerifyPassword(PasswordHash hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(providedPassword))
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword.Value, providedPassword);

        return result switch
        {
            PasswordVerificationResult.Success => true,
            PasswordVerificationResult.SuccessRehashNeeded => true, // Still valid, but should be rehashed
            _ => false
        };
    }

    /// <inheritdoc />
    public PasswordStrengthResult ValidateStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return PasswordStrengthResult.Invalid(["Password is required."]);
        }

        var errors = new List<string>();

        // Check minimum length
        if (password.Length < _policyOptions.MinimumLength)
        {
            errors.Add($"Password must be at least {_policyOptions.MinimumLength} characters.");
        }

        // Check maximum length (prevent DoS)
        if (password.Length > 128)
        {
            errors.Add("Password cannot exceed 128 characters.");
        }

        // Check for uppercase
        if (_policyOptions.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        // Check for lowercase
        if (_policyOptions.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        // Check for digit
        if (_policyOptions.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }

        // Check for special character
        if (_policyOptions.RequireSpecialCharacter && !password.Any(IsSpecialCharacter))
        {
            errors.Add("Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;:',.<>?/).");
        }

        if (errors.Count > 0)
        {
            return PasswordStrengthResult.Invalid(errors, GenerateSuggestions(password));
        }

        // Calculate strength score
        var score = CalculateStrengthScore(password);
        var strength = score switch
        {
            0 => "Very Weak",
            1 => "Weak",
            2 => "Fair",
            3 => "Strong",
            _ => "Very Strong"
        };

        return PasswordStrengthResult.Valid(score, strength);
    }

    private static bool IsSpecialCharacter(char c)
    {
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:',.<>?/`~\\\"";
        return specialChars.Contains(c);
    }

    private static int CalculateStrengthScore(string password)
    {
        var score = 0;

        // Length bonus
        if (password.Length >= 12)
        {
            score++;
        }

        if (password.Length >= 16)
        {
            score++;
        }

        // Character variety bonus
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(IsSpecialCharacter);

        var varietyCount = new[] { hasUpper, hasLower, hasDigit, hasSpecial }.Count(x => x);
        if (varietyCount >= 3)
        {
            score++;
        }

        if (varietyCount == 4)
        {
            score++;
        }

        // Cap at 4
        return Math.Min(score, 4);
    }

    private static List<string> GenerateSuggestions(string password)
    {
        var suggestions = new List<string>();

        if (password.Length < 16)
        {
            suggestions.Add("Consider using a longer password for better security.");
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower))
        {
            suggestions.Add("Mix uppercase and lowercase letters.");
        }

        if (!password.Any(char.IsDigit))
        {
            suggestions.Add("Add some numbers to your password.");
        }

        if (!password.Any(IsSpecialCharacter))
        {
            suggestions.Add("Include special characters like !@#$%.");
        }

        // Check for common patterns
        if (HasCommonPatterns(password))
        {
            suggestions.Add("Avoid common patterns like '123', 'abc', or keyboard sequences.");
        }

        return suggestions;
    }

    private static bool HasCommonPatterns(string password)
    {
        var lowerPassword = password.ToLowerInvariant();

        // Common sequences
        string[] commonPatterns =
        [
            "123", "234", "345", "456", "567", "678", "789",
            "abc", "bcd", "cde", "def", "efg",
            "qwerty", "asdf", "zxcv",
            "password", "letmein", "welcome"
        ];

        return commonPatterns.Any(pattern => lowerPassword.Contains(pattern));
    }
}

/// <summary>
/// Password policy configuration options.
/// </summary>
public sealed class PasswordPolicyOptions
{
    /// <summary>
    /// Configuration section name for password policy options.
    /// </summary>
    public const string SectionName = "Authentication:PasswordPolicy";

    /// <summary>
    /// Minimum password length (default: 12).
    /// </summary>
    public int MinimumLength { get; set; } = 12;

    /// <summary>
    /// Require at least one uppercase letter.
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Require at least one lowercase letter.
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Require at least one digit.
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Require at least one special character.
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = true;
}
