using SRS_Stock.Core.Theming;

namespace SRS_Stock.Theming;

public sealed class MauiThemeApplicator : IThemeApplicator
{
    public void Apply(ThemePreference preference)
    {
        if (Application.Current is null)
            return;

        Application.Current.UserAppTheme = preference switch
        {
            ThemePreference.Light => AppTheme.Light,
            ThemePreference.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}
