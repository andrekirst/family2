using FamilyHub.Common.Application;

namespace FamilyHub.Api.Common.Infrastructure.ErrorMapping;

/// <summary>
/// Extension methods for mapping <see cref="Result{T}"/> to Minimal API <see cref="IResult"/>.
/// Bridges the domain Result pattern with HTTP responses using RFC 9457 ProblemDetails for errors.
/// </summary>
public static class ResultToIResultExtensions
{
    /// <summary>
    /// Maps a <see cref="Result{T}"/> to an <see cref="IResult"/>.
    /// On success, applies the <paramref name="onSuccess"/> mapping function.
    /// On failure, maps the <see cref="DomainError"/> to an RFC 9457 ProblemDetails response.
    /// </summary>
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, IResult> onSuccess)
    {
        return result.Match(
            onSuccess,
            error => DomainErrorToProblemDetailsMapper.ToProblemDetails(error));
    }
}
