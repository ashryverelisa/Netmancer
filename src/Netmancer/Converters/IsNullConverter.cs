using System.Globalization;

namespace Netmancer.Converters;

/// <summary>
/// Returns true when the value is null (or empty string), false otherwise.
/// Useful for showing/hiding fallback content.
/// </summary>
public class IsNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null or ("");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

