namespace SRS_Stock.Core.Localization;

public interface ILanguageService
{
    IReadOnlyList<AppLanguage> Languages { get; }

    AppLanguage Current { get; }

    event EventHandler? Changed;

    void Apply(string cultureName);
}
