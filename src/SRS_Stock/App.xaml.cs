using Microsoft.Extensions.DependencyInjection;
using SRS_Stock.Core.Localization;
using SRS_Stock.Core.Resources;
using SRS_Stock.Core.Theming;
using SRS_Stock.Localization;

namespace SRS_Stock;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
        InitializeComponent();
        services.GetRequiredService<IThemeService>();
        Resources.Add(UiLocalizer.ResourceKey, services.GetRequiredService<UiLocalizer>());
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var language = _services.GetRequiredService<ILanguageService>();
        var window = new Window(_services.GetRequiredService<AppShell>())
        {
            Title = AppStrings.AppTitle,
        };
        language.Changed += (_, _) => window.Title = AppStrings.AppTitle;
        return window;
    }
}
