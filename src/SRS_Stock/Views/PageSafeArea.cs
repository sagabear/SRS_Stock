using Microsoft.Maui.Devices;

namespace SRS_Stock.Views;

static class PageSafeArea
{
    public static void Apply(ContentPage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        var platform = DeviceInfo.Current.Platform;

        // iOS and Mac Catalyst report the keyboard as a safe-area inset.
        // Android resizes the window instead, so only the notch and system bars are inset.
        var regions = platform == DevicePlatform.iOS || platform == DevicePlatform.MacCatalyst
            ? SafeAreaRegions.All
            : SafeAreaRegions.Container;

        page.SafeAreaEdges = new SafeAreaEdges(regions);
    }
}
