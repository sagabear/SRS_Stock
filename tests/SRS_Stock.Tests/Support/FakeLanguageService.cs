using SRS_Stock.Core.Localization;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeLanguageService : ILanguageService
{
    public IReadOnlyList<AppLanguage> Languages { get; } = AppLanguages.All;

    public AppLanguage Current { get; private set; } = AppLanguages.All[0];

    public List<string> Applied { get; } = [];

    public event EventHandler? Changed;

    public void Apply(string cultureName)
    {
        var language = Languages.Single(item =>
            string.Equals(item.CultureName, cultureName, StringComparison.OrdinalIgnoreCase));
        if (language.CultureName == Current.CultureName)
            return;

        Applied.Add(language.CultureName);
        Current = language;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
