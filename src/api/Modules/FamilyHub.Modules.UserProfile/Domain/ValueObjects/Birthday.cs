namespace FamilyHub.Modules.UserProfile.Domain.ValueObjects;

/// <summary>
/// Represents a user's birthday.
/// Strongly-typed DateOnly value object enforcing domain validation rules.
/// Must be in the past, maximum 150 years ago.
/// </summary>
[ValueObject<DateOnly>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public readonly partial struct Birthday
{
    /// <summary>
    /// Maximum number of years in the past a birthday can be.
    /// </summary>
    private const int MaxYearsAgo = 150;

    private static Validation Validate(DateOnly value)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (value > today)
        {
            return Validation.Invalid("Birthday cannot be in the future.");
        }

        if (value < today.AddYears(-MaxYearsAgo))
        {
            return Validation.Invalid($"Birthday cannot be more than {MaxYearsAgo} years ago.");
        }

        return Validation.Ok;
    }

    /// <summary>
    /// Calculates the current age based on this birthday.
    /// </summary>
    public int CalculateAge()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - Value.Year;

        // Adjust if birthday hasn't occurred yet this year
        if (Value > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
