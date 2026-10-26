using System.Globalization;
using SRS_Stock.Core.Localization;
using SRS_Stock.Tests.Support;

namespace SRS_Stock.Tests;

public sealed class LanguageServiceTests
{
    [Fact]
    public void SavedLanguage_IsAppliedWithoutWritingItAgain()
    {
        var preference = new FakeLanguagePreference { CultureName = "en-US" };
        var culture = new FakeUiCulture();

        var service = Create(preference, culture);

        Assert.Equal("en", service.Current.CultureName);
        Assert.Equal(["en"], culture.Applied);
        Assert.Equal(0, preference.SetCount);
    }

    [Fact]
    public void SupportedSystemLanguage_IsUsedWhenNothingIsSaved()
    {
        var culture = new FakeUiCulture { Current = CultureInfo.GetCultureInfo("en-US") };

        var service = Create(culture: culture);

        Assert.Equal("en", service.Current.CultureName);
        Assert.Equal(["en"], culture.Applied);
    }

    [Fact]
    public void UnsupportedSystemLanguage_FallsBackToHungarian()
    {
        var culture = new FakeUiCulture();
        var service = Create(culture: culture);

        Assert.Equal("hu", service.Current.CultureName);
        Assert.Equal(["hu"], culture.Applied);
    }

    [Fact]
    public void Apply_PersistsTheLanguageAndNotifies()
    {
        var preference = new FakeLanguagePreference();
        var culture = new FakeUiCulture();
        var service = Create(preference, culture);
        var raised = 0;
        EventHandler handler = (_, _) => raised++;
        service.Changed += handler;

        service.Apply("en");

        Assert.Equal("en", service.Current.CultureName);
        Assert.Equal("en", preference.CultureName);
        Assert.Equal(1, preference.SetCount);
        Assert.Equal(1, raised);
        Assert.Equal(["hu", "en"], culture.Applied);
    }

    [Fact]
    public void Apply_SameLanguage_DoesNothing()
    {
        var preference = new FakeLanguagePreference();
        var service = Create(preference);

        service.Apply("hu");

        Assert.Equal(0, preference.SetCount);
    }

    [Fact]
    public void Apply_UnknownLanguage_Throws()
    {
        var service = Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Apply("de"));
    }

    private static LanguageService Create(
        FakeLanguagePreference? preference = null,
        FakeUiCulture? culture = null) =>
        new(
            preference ?? new FakeLanguagePreference(),
            culture ?? new FakeUiCulture(),
            new ListLogger<LanguageService>());
}
