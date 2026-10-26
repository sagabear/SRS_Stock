using System.Globalization;

namespace SRS_Stock.Converters;

public sealed class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasText = value is string text && text.Trim().Length > 0;
        var invert = parameter is string flag && flag.Equals("invert", StringComparison.OrdinalIgnoreCase);
        return invert ? !hasText : hasText;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
