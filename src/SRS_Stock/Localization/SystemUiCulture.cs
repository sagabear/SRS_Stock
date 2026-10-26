using System.Globalization;
using SRS_Stock.Core.Localization;

namespace SRS_Stock.Localization;

public sealed class SystemUiCulture : IUiCulture
{
    public CultureInfo Current => CultureInfo.CurrentUICulture;

    public void Apply(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
