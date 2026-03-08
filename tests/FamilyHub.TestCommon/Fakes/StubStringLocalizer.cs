using Microsoft.Extensions.Localization;

namespace FamilyHub.TestCommon.Fakes;

/// <summary>
/// Simple stub that returns the key as the localized value.
/// Used for testing validators that take IStringLocalizer dependencies.
/// </summary>
public sealed class StubStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);
    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
