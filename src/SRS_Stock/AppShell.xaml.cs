using Microsoft.Extensions.DependencyInjection;
using SRS_Stock.Core.Navigation;
using SRS_Stock.Core.Resources;
using SRS_Stock.Localization;
using SRS_Stock.Views;

namespace SRS_Stock;

public partial class AppShell : Shell
{
    private const double LockedMenuWidth = 1000;

    public AppShell(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        InitializeComponent();

        var localizer = services.GetRequiredService<UiLocalizer>();
        AddPage(localizer, nameof(AppStrings.MenuInventory), Routes.ProductList, services.GetRequiredService<ProductListPage>());
        AddPage(localizer, nameof(AppStrings.MenuSettings), Routes.Settings, services.GetRequiredService<SettingsPage>());
        AddPage(localizer, nameof(AppStrings.MenuAbout), Routes.About, services.GetRequiredService<AboutPage>());

        Routing.RegisterRoute(Routes.ProductEdit, typeof(ProductEditPage));

        var version = AppInfo.Current.VersionString;
        VersionLabel.Text = string.IsNullOrWhiteSpace(version) ? "1.0" : version;
        FlyoutVersion.SafeAreaEdges = new SafeAreaEdges(
            SafeAreaRegions.None,
            SafeAreaRegions.None,
            SafeAreaRegions.None,
            SafeAreaRegions.Container);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width < 1)
            return;

        var behavior = width >= LockedMenuWidth
            ? Microsoft.Maui.FlyoutBehavior.Locked
            : Microsoft.Maui.FlyoutBehavior.Flyout;
        if (FlyoutBehavior != behavior)
            FlyoutBehavior = behavior;
    }

    private void AddPage(UiLocalizer localizer, string titleKey, string route, Page page)
    {
        var content = new ShellContent { Content = page };
        var section = new ShellSection();
        section.Items.Add(content);

        var item = new FlyoutItem { Route = route };
        item.SetBinding(BaseShellItem.TitleProperty, TitleBinding(localizer, titleKey));
        item.Items.Add(section);
        Items.Add(item);
    }

    private static Binding TitleBinding(UiLocalizer localizer, string titleKey) =>
        new()
        {
            Path = $"[{titleKey}]",
            Mode = BindingMode.OneWay,
            Source = localizer,
        };
}
