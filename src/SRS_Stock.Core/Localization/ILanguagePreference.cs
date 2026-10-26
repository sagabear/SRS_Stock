namespace SRS_Stock.Core.Localization;

public interface ILanguagePreference
{
    string? GetCultureName();

    void SetCultureName(string cultureName);
}
