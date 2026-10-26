using System.Globalization;

namespace SRS_Stock.Converters;

public sealed class IsZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            decimal number => number == 0m,
            double number => number == 0d,
            float number => number == 0f,
            int number => number == 0,
            _ => false,
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
