using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YouTubeMusicAPI.Sample.WPF.Converters;

public class UrlToBrushConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url))
            return Brushes.Gray;

        try
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return new ImageBrush(bitmap)
            {
                Stretch = Stretch.UniformToFill
            };
        }
        catch
        {
            return Brushes.Gray;
        }

    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotImplementedException();
}