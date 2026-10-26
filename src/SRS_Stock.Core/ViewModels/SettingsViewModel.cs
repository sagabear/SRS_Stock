using CommunityToolkit.Mvvm.ComponentModel;
using SRS_Stock.Core.Localization;
using SRS_Stock.Core.Resources;
using SRS_Stock.Core.Theming;

namespace SRS_Stock.Core.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILanguageService _language;
    private readonly IThemeService _theme;
    private readonly EventHandler _onLanguageChanged;
    private readonly EventHandler _onThemeChanged;

    public SettingsViewModel(ILanguageService language, IThemeService theme)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(theme);
        _language = language;
        _theme = theme;
        _selectedLanguage = language.Current;
        RefreshThemeOptions();
        _onLanguageChanged = HandleLanguageChanged;
        _onThemeChanged = HandleThemeChanged;
        language.Changed += _onLanguageChanged;
        theme.Changed += _onThemeChanged;
    }

    public IReadOnlyList<AppLanguage> Languages => _language.Languages;

    public IReadOnlyList<ThemeOption> ThemeOptions { get; private set; } = [];

    [ObservableProperty]
    private AppLanguage? _selectedLanguage;

    [ObservableProperty]
    private ThemeOption? _selectedTheme;

    partial void OnSelectedLanguageChanged(AppLanguage? value)
    {
        if (value is null || value.CultureName == _language.Current.CultureName)
            return;

        _language.Apply(value.CultureName);
    }

    partial void OnSelectedThemeChanged(ThemeOption? value)
    {
        if (value is null || value.Preference == _theme.Current)
            return;

        _theme.Apply(value.Preference);
    }

    private void HandleLanguageChanged(object? sender, EventArgs e)
    {
        if (SelectedLanguage?.CultureName != _language.Current.CultureName)
            SelectedLanguage = _language.Current;

        RefreshThemeOptions();
    }

    private void HandleThemeChanged(object? sender, EventArgs e)
    {
        if (SelectedTheme?.Preference == _theme.Current)
            return;

        SelectedTheme = ThemeOptions.FirstOrDefault(option => option.Preference == _theme.Current);
    }

    private void RefreshThemeOptions()
    {
        var selected = SelectedTheme?.Preference ?? _theme.Current;
        ThemeOptions =
        [
            new(ThemePreference.System, AppStrings.ThemeSystem),
            new(ThemePreference.Light, AppStrings.ThemeLight),
            new(ThemePreference.Dark, AppStrings.ThemeDark),
        ];
        OnPropertyChanged(nameof(ThemeOptions));

        var match = ThemeOptions.First(option => option.Preference == selected);
        if (SelectedTheme != match)
            SelectedTheme = match;
    }
}
