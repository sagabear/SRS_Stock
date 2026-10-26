using SRS_Stock.Core.Theming;
using SRS_Stock.Core.ViewModels;
using SRS_Stock.Tests.Support;

namespace SRS_Stock.Tests;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void SelectingLanguage_AppliesIt()
    {
        var language = new FakeLanguageService();
        var viewModel = new SettingsViewModel(language, new FakeThemeService());

        viewModel.SelectedLanguage = language.Languages[1];

        Assert.Equal(["en"], language.Applied);
        Assert.Equal("en", viewModel.SelectedLanguage?.CultureName);
    }

    [Fact]
    public void ExternalLanguageChange_UpdatesTheSelection()
    {
        var language = new FakeLanguageService();
        var viewModel = new SettingsViewModel(language, new FakeThemeService());

        language.Apply("en");

        Assert.Equal("en", viewModel.SelectedLanguage?.CultureName);
        Assert.Equal(["en"], language.Applied);
    }

    [Fact]
    public void SelectingTheme_AppliesIt()
    {
        var theme = new FakeThemeService();
        var viewModel = new SettingsViewModel(new FakeLanguageService(), theme);

        viewModel.SelectedTheme = viewModel.ThemeOptions.Single(option => option.Preference == ThemePreference.Dark);

        Assert.Equal([ThemePreference.Dark], theme.Applied);
        Assert.Equal(ThemePreference.Dark, viewModel.SelectedTheme?.Preference);
    }
}
