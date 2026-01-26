using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.Tests.Unit.UserProfile.Domain.ValueObjects;

/// <summary>
/// Unit tests for Birthday value object.
/// </summary>
public class BirthdayTests
{
    [Fact]
    public void From_WithValidPastDate_ShouldSucceed()
    {
        // Arrange
        var pastDate = new DateOnly(1990, 6, 15);

        // Act
        var birthday = Birthday.From(pastDate);

        // Assert
        birthday.Value.Should().Be(pastDate);
    }

    [Fact]
    public void From_WithTodayDate_ShouldSucceed()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var birthday = Birthday.From(today);

        // Assert
        birthday.Value.Should().Be(today);
    }

    [Fact]
    public void From_WithFutureDate_ShouldThrow()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        // Act
        var act = () => Birthday.From(futureDate);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Birthday cannot be in the future*");
    }

    [Fact]
    public void From_WithDateMoreThan150YearsAgo_ShouldThrow()
    {
        // Arrange
        var veryOldDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-151));

        // Act
        var act = () => Birthday.From(veryOldDate);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>()
            .WithMessage("*Birthday cannot be more than 150 years ago*");
    }

    [Fact]
    public void From_WithDateExactly150YearsAgo_ShouldSucceed()
    {
        // Arrange
        var oldDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-150));

        // Act
        var birthday = Birthday.From(oldDate);

        // Assert
        birthday.Value.Should().Be(oldDate);
    }

    [Fact]
    public void CalculateAge_ShouldReturnCorrectAge()
    {
        // Arrange
        var today = DateTime.Today;
        var birthDate = new DateOnly(today.Year - 30, today.Month, today.Day);
        var birthday = Birthday.From(birthDate);

        // Act
        var age = birthday.CalculateAge();

        // Assert
        age.Should().Be(30);
    }

    [Fact]
    public void CalculateAge_BeforeBirthdayThisYear_ShouldReturnCorrectAge()
    {
        // Arrange - birthday hasn't happened yet this year
        var today = DateTime.Today;
        var birthDate = new DateOnly(today.Year - 30, 12, 31); // December 31
        if (today.Month == 12 && today.Day == 31)
        {
            // Edge case: if today is Dec 31, use a different test date
            birthDate = new DateOnly(today.Year - 30, today.Month, today.Day);
        }
        var birthday = Birthday.From(birthDate);

        // Act
        var age = birthday.CalculateAge();

        // Assert - Should be 29 if birthday hasn't happened yet this year
        if (new DateOnly(today.Year, today.Month, today.Day) < new DateOnly(today.Year, birthDate.Month, birthDate.Day))
        {
            age.Should().Be(29);
        }
        else
        {
            age.Should().Be(30);
        }
    }

    [Fact]
    public void Equals_WithSameDate_ShouldBeEqual()
    {
        // Arrange
        var date = new DateOnly(1990, 6, 15);
        var birthday1 = Birthday.From(date);
        var birthday2 = Birthday.From(date);

        // Act & Assert
        birthday1.Should().Be(birthday2);
        (birthday1 == birthday2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentDates_ShouldNotBeEqual()
    {
        // Arrange
        var birthday1 = Birthday.From(new DateOnly(1990, 6, 15));
        var birthday2 = Birthday.From(new DateOnly(1990, 6, 16));

        // Act & Assert
        birthday1.Should().NotBe(birthday2);
        (birthday1 != birthday2).Should().BeTrue();
    }
}
