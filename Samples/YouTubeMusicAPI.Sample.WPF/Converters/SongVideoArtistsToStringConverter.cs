using System.Globalization;
using System.Windows.Data;
using YouTubeMusicAPI.Models;

namespace YouTubeMusicAPI.Sample.WPF.Converters;

public class SongVideoArtistsToStringConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is not NamedEntity[] artists)
            return Binding.DoNothing;

        string text = string.Join(", ", artists.Select(artist => artist.Name));
        return text;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotImplementedException();
}