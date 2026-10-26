namespace SRS_Stock.Core.Theming;

public interface IThemePreference
{
    ThemePreference? Get();

    void Set(ThemePreference preference);
}
