using SRS_Stock.Core.Theming;
using SRS_Stock.Tests.Support;

namespace SRS_Stock.Tests;

public sealed class ThemeServiceTests
{
    [Fact]
    public void StartsOnSystemWhenNothingIsSaved()
    {
        var preference = new FakeThemePreference();
        var applicator = new FakeThemeApplicator();

        var service = Create(preference, applicator);

        Assert.Equal(ThemePreference.System, service.Current);
        Assert.Equal([ThemePreference.System], applicator.Applied);
        Assert.Equal(0, preference.SetCount);
    }

    [Fact]
    public void SavedTheme_IsAppliedWithoutWritingItAgain()
    {
        var preference = new FakeThemePreference { Saved = ThemePreference.Dark };
        var applicator = new FakeThemeApplicator();

        var service = Create(preference, applicator);

        Assert.Equal(ThemePreference.Dark, service.Current);
        Assert.Equal([ThemePreference.Dark], applicator.Applied);
        Assert.Equal(0, preference.SetCount);
    }

    [Fact]
    public void Apply_PersistsTheThemeAndNotifies()
    {
        var preference = new FakeThemePreference();
        var applicator = new FakeThemeApplicator();
        var service = Create(preference, applicator);
        var raised = 0;
        service.Changed += (_, _) => raised++;

        service.Apply(ThemePreference.Light);

        Assert.Equal(ThemePreference.Light, service.Current);
        Assert.Equal(ThemePreference.Light, preference.Saved);
        Assert.Equal(1, preference.SetCount);
        Assert.Equal(1, raised);
        Assert.Equal([ThemePreference.System, ThemePreference.Light], applicator.Applied);
    }

    [Fact]
    public void Apply_SameTheme_DoesNothing()
    {
        var preference = new FakeThemePreference();
        var service = Create(preference, new FakeThemeApplicator());

        service.Apply(ThemePreference.System);

        Assert.Equal(0, preference.SetCount);
    }

    private static ThemeService Create(FakeThemePreference preference, FakeThemeApplicator applicator) =>
        new(preference, applicator, new ListLogger<ThemeService>());
}
