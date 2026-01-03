namespace FamilyHub.Tests.Unit.Fixtures;

/// <summary>
/// Fake TimeProvider for deterministic time control in tests.
/// </summary>
public sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _currentTime;

    public FakeTimeProvider(DateTimeOffset? initialTime = null)
    {
        _currentTime = initialTime ?? new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    }

    public override DateTimeOffset GetUtcNow() => _currentTime;

    public void Advance(TimeSpan duration)
    {
        _currentTime = _currentTime.Add(duration);
    }

    public void SetTime(DateTimeOffset time)
    {
        _currentTime = time;
    }
}
