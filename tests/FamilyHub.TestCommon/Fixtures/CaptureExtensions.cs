using NSubstitute;

namespace FamilyHub.TestCommon.Fixtures;

/// <summary>
/// Extensions for capturing arguments passed to NSubstitute mocks.
/// Use for complex multi-property assertions with FluentAssertions.
/// For simple single-property checks, prefer Arg.Is&lt;T&gt; inline.
/// </summary>
public static class CaptureExtensions
{
    /// <summary>
    /// Captures all arguments of a specific type passed to a method.
    /// Usage: var captured = mock.Capture&lt;Family&gt;(x => x.AddAsync(default!, default));
    /// Then: captured.Single().Name.Should().Be(expected);
    /// </summary>
    public static List<T> Capture<T>(this object mock, Action<object> methodCall) where T : class
    {
        var captured = new List<T>();
        methodCall(mock);
        mock.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<T>()
            .ToList()
            .ForEach(captured.Add);
        return captured;
    }
}
