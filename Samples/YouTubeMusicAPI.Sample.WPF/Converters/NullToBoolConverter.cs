using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YouTubeMusicAPI.Sample.WPF.Converters;

public class NullToBoolConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (parameter is bool isInverted && isInverted)
            return value is null;

        return value is not null;

    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotImplementedException();
}