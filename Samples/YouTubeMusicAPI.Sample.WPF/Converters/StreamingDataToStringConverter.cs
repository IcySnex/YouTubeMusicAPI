using System.Globalization;
using System.Windows.Data;
using YouTubeMusicAPI.Models.Streaming;

namespace YouTubeMusicAPI.Sample.WPF.Converters;

public class StreamingDataToStringConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is not StreamingData streamingData)
            return Binding.DoNothing;

        AudioStreamInfo highestAudioStreamInfo = streamingData.StreamInfo
          .OfType<AudioStreamInfo>()
          .OrderByDescending(info => info.Bitrate)
          .First();

        string streamingInfo = "";

        streamingInfo += $"- Is Live Content: {streamingData.IsLiveContent}\n";
        streamingInfo += $"- Expires In: {streamingData.ExpiresIn}\n";
        streamingInfo += $"- HLS Manifest URL: {streamingData.HlsManifestUrl ?? "N/A"}\n\n";

        streamingInfo += $"Highest Audio Stream\n";
        streamingInfo += $"- Itag: {highestAudioStreamInfo.Itag}\n";
        streamingInfo += $"- Container Codecs: {highestAudioStreamInfo.Container.Codecs}\n";
        streamingInfo += $"- Container Format: {highestAudioStreamInfo.Container.Format}\n";
        streamingInfo += $"- Last Modified At: {highestAudioStreamInfo.LastModifedAt}\n";
        streamingInfo += $"- Content Lenght: {highestAudioStreamInfo.ContentLenght}\n";
        streamingInfo += $"- Bitrate: {highestAudioStreamInfo.Bitrate}\n";
        streamingInfo += $"- Quality: {highestAudioStreamInfo.Quality}\n";
        streamingInfo += $"- Sample Rate: {highestAudioStreamInfo.SampleRate}\n";
        streamingInfo += $"- Channels Count: {highestAudioStreamInfo.Channels}\n";
        streamingInfo += $"- Loudness in Db: {highestAudioStreamInfo.LoudnessDb}\n";
        streamingInfo += $"- URL: {highestAudioStreamInfo.Url}";

        return streamingInfo;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotImplementedException();
}