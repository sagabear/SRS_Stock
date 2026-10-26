using SRS_Stock.Core.Theming;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeThemeService : IThemeService
{
    public ThemePreference Current { get; private set; } = ThemePreference.System;

    public List<ThemePreference> Applied { get; } = [];

    public event EventHandler? Changed;

    public void Apply(ThemePreference preference)
    {
        if (preference == Current)
            return;

        Applied.Add(preference);
        Current = preference;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
