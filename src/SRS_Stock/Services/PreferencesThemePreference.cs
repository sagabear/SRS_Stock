using SRS_Stock.Core.Theming;

namespace SRS_Stock.Services;

public sealed class PreferencesThemePreference : IThemePreference
{
    private const string Key = "ui_theme";

    public ThemePreference? Get()
    {
        var value = Preferences.Default.Get(Key, string.Empty);
        return Enum.TryParse<ThemePreference>(value, ignoreCase: true, out var preference)
            && Enum.IsDefined(preference)
            ? preference
            : null;
    }

    public void Set(ThemePreference preference)
    {
        if (!Enum.IsDefined(preference))
            throw new ArgumentOutOfRangeException(nameof(preference), preference, "Unsupported theme.");

        Preferences.Default.Set(Key, preference.ToString());
    }
}
