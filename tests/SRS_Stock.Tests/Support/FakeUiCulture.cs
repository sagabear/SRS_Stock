using System.Globalization;
using SRS_Stock.Core.Localization;

namespace SRS_Stock.Tests.Support;

internal sealed class FakeUiCulture : IUiCulture
{
    public CultureInfo Current { get; set; } = CultureInfo.GetCultureInfo("fr-FR");

    public List<string> Applied { get; } = [];

    public void Apply(CultureInfo culture)
    {
        Applied.Add(culture.Name);
        Current = culture;
    }
}
