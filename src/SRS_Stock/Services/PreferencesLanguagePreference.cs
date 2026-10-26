using SRS_Stock.Core.Localization;

namespace SRS_Stock.Services;

public sealed class PreferencesLanguagePreference : ILanguagePreference
{
    private const string Key = "ui_language";

    public string? GetCultureName()
    {
        var value = Preferences.Default.Get(Key, string.Empty);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public void SetCultureName(string cultureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);
        Preferences.Default.Set(Key, cultureName);
    }
}
