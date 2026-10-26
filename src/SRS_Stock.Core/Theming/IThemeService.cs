namespace SRS_Stock.Core.Theming;

public interface IThemeService
{
    ThemePreference Current { get; }

    event EventHandler? Changed;

    void Apply(ThemePreference preference);
}
