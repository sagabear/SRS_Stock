using SRS_Stock.Core.Theming;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeThemePreference : IThemePreference
{
    public ThemePreference? Saved { get; set; }

    public int SetCount { get; private set; }

    public ThemePreference? Get() => Saved;

    public void Set(ThemePreference preference)
    {
        SetCount++;
        Saved = preference;
    }
}
