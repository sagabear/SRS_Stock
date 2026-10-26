using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using SRS_Stock.Core.Interaction;
using SRS_Stock.Core.Localization;
using SRS_Stock.Core.Navigation;
using SRS_Stock.Core.Services;
using SRS_Stock.Core.Theming;
using SRS_Stock.Core.Threading;
using SRS_Stock.Core.ViewModels;
using SRS_Stock.Localization;
using SRS_Stock.Services;
using SRS_Stock.Theming;
using SRS_Stock.Threading;
using SRS_Stock.Views;

namespace SRS_Stock;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IUiDispatcher, MainThreadDispatcher>();
        builder.Services.AddSingleton<ILanguagePreference, PreferencesLanguagePreference>();
        builder.Services.AddSingleton<IUiCulture, SystemUiCulture>();
        builder.Services.AddSingleton<ILanguageService, LanguageService>();
        builder.Services.AddSingleton<IThemePreference, PreferencesThemePreference>();
        builder.Services.AddSingleton<IThemeApplicator, MauiThemeApplicator>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<UiLocalizer>();
        builder.Services.AddSingleton<IAppNavigator, ShellAppNavigator>();
        builder.Services.AddSingleton<IUserPrompt, ShellUserPrompt>();
        builder.Services.AddSingleton<IProductRepository>(static services =>
            new SqliteProductRepository(
                Path.Combine(FileSystem.AppDataDirectory, "leltar.db"),
                services.GetRequiredService<ILogger<SqliteProductRepository>>()));
        builder.Services.AddTransient<ProductListViewModel>();
        builder.Services.AddTransient<ProductEditViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AboutViewModel>(_ => new AboutViewModel(AppVersion()));
        builder.Services.AddTransient<ProductListPage>();
        builder.Services.AddTransient<ProductEditPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddSingleton<AppShell>();

        ConfigureEntries();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static string AppVersion()
    {
        var version = AppInfo.Current.VersionString;
        return string.IsNullOrWhiteSpace(version) ? "1.0" : version;
    }

    private static void ConfigureEntries()
    {
        EntryHandler.Mapper.AppendToMapping("Borderless", (handler, _) =>
        {
#if WINDOWS
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
#elif ANDROID
            handler.PlatformView.Background = null;
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#endif
        });
    }
}
