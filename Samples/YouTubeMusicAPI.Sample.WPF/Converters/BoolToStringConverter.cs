using System.Globalization;
using System.Windows.Data;

namespace YouTubeMusicAPI.Sample.WPF.Converters;

public class BoolToStringConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is not bool boolValue)
            return Binding.DoNothing;

        string trueText = "True";
        string falseText = "False";

        if (parameter is string param)
        {
            string[] parts = param.Split('|');

            trueText = parts[0];
            falseText = parts[1];
        }

        return boolValue ? trueText : falseText;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotImplementedException();
}