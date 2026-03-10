using AutoFixture;

namespace FamilyHub.TestCommon.Fixtures;

/// <summary>
/// Extension methods for registering Vogen value objects with AutoFixture.
/// </summary>
public static class FixtureExtensions
{
    private static int _counter;

    /// <summary>
    /// Registers a Guid-based Vogen value object with AutoFixture.
    /// </summary>
    public static void RegisterVogenId<T>(this IFixture fixture, Func<Guid, T> factory)
    {
        fixture.Customize<T>(c => c.FromFactory(() => factory(Guid.NewGuid())));
    }

    /// <summary>
    /// Registers a string-based Vogen value object with AutoFixture.
    /// Uses an optional format string with an incrementing counter for uniqueness.
    /// </summary>
    public static void RegisterVogenString<T>(
        this IFixture fixture,
        Func<string, T> factory,
        string? format = null)
    {
        fixture.Customize<T>(c => c.FromFactory(() =>
        {
            var counter = Interlocked.Increment(ref _counter);
            var value = format is not null
                ? string.Format(format, counter)
                : $"test-value-{counter}";
            return factory(value);
        }));
    }
}
