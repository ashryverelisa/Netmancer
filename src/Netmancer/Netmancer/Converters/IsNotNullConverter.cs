using System.Globalization;
using Avalonia.Data.Converters;

namespace Netmancer.Converters;

/// <summary>
/// Returns true when the value is not null (and not empty string), false otherwise.
/// Inverse of <see cref="IsNullConverter"/>.
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null and not "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

