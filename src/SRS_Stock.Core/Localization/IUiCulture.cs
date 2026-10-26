using System.Globalization;

namespace SRS_Stock.Core.Localization;

public interface IUiCulture
{
    CultureInfo Current { get; }

    void Apply(CultureInfo culture);
}
