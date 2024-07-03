using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Models;
using YouTubeMusicAPI.Models.Shelf;
using YouTubeMusicAPI.Types;

namespace YouTubeMusicAPI.Internal;
/// <summary>
/// Contains methods to parse generic data from json tokens
/// </summary>
internal static class Parser
{
    /// <summary>
    /// Parses an exact string into a TimeSpan
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>A new TimeSpan</returns>
    /// <exception cref="ArgumentException">value has an invalid format</exception>
    public static TimeSpan TimeSpanExact(
        string value)
    {
        if (TimeSpan.TryParseExact(value, @"m\:ss", null, out TimeSpan timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"h\:mm\:ss", null, out timeSpan))
            return timeSpan;
        if (TimeSpan.TryParseExact(value, @"hh\:mm\:ss", null, out timeSpan))
            return timeSpan;

        throw new ArgumentException("value has an invalid format");
    }

    /// <summary>
    /// Parses a long string into a TimeSpan
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>A new TimeSpan</returns>
    public static TimeSpan TimeSpanLong(
        string value)
    {
        Regex hourPattern = new(@"(\d+)\s*\+?\s*hour", RegexOptions.IgnoreCase);
        Regex minutePattern = new(@"(\d+)\s*\+?\s*minute", RegexOptions.IgnoreCase);
        Regex secondPattern = new(@"(\d+)\s*\+?\s*second", RegexOptions.IgnoreCase);

        Match hourMatch = hourPattern.Match(value);
        Match minuteMatch = minutePattern.Match(value);
        Match secondMatch = secondPattern.Match(value);

        return new(
            hourMatch.Success ? int.Parse(hourMatch.Groups[1].Value) : 0,
            minuteMatch.Success ? int.Parse(minuteMatch.Groups[1].Value) : 0,
            secondMatch.Success ? int.Parse(secondMatch.Groups[1].Value) : 0);
    }


    /// <summary>
    /// Parses thumbnails data from the json token in the specified path
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <param name="tokenPath">The json token path</param>
    /// <returns>An array of thumbnails</returns>
    public static Thumbnail[] GetThumbnails(
        JToken jsonToken,
        string tokenPath = "thumbnail.musicThumbnailRenderer.thumbnail.thumbnails")
    {
        // Parse container from json token
        JToken? thumbnails = jsonToken.SelectToken(tokenPath);
        if (thumbnails is null)
            return [];

        List<Thumbnail> result = [];
        foreach (JToken thumbnail in thumbnails)
        {
            // Parse info from container
            string? url = thumbnail.SelectToken("url")?.ToString();
            string? width = thumbnail.SelectToken("width")?.ToString();
            string? height = thumbnail.SelectToken("height")?.ToString();

            if (url is null)
                continue;

            result.Add(new(
                url,
                width is null ? 0 : int.Parse(width),
                height is null ? 0 : int.Parse(height)));
        }

        // Return result
        return [.. result];
    }

    /// <summary>
    /// Parses artists data from the json token in the specified path
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <param name="tokenPath">The json token path</param>
    /// <param name="startIndex">The index to start the runs container</param>
    /// <param name="trimBy">Trim the last items of runs container</param>
    /// <returns>An array of artists</returns>
    public static ShelfItem[] GetArtists(
        JToken jsonToken,
        string tokenPath,
        int startIndex = 0,
        int trimBy = 0)
    {
        // Parse container from json token
        JToken[] runs = jsonToken.SelectToken(tokenPath)?.ToArray() ?? throw new ArgumentNullException(null, "One or more values of item is null");
        
        List<ShelfItem> result = [];
        for (int i = startIndex; i < runs.Length - trimBy; i++)
        {
            JToken run = runs[i];

            // Parse info from container
            string? artist = run.SelectToken("text")?.ToString()?.Trim();
            string? artistId = run.SelectToken("navigationEndpoint.browseEndpoint.browseId")?.ToString();

            if (artist is null || artist == "," || artist == "&" || artist == "•")
                continue;

            result.Add(new(
                artist,
                artistId,
                ShelfKind.Artists));
        }

        return [.. result];
    }

    /// <summary>
    /// Parses simple artists data from the json token in the specified path
    /// </summary>
    /// <param name="jsonToken">The json token containing the item data</param>
    /// <returns>An array of artists</returns>
    public static ShelfItem[] GetArtistsSimple(
        JToken jsonToken)
    {
        // Parse artist names from json token
        string? artistNames = jsonToken.SelectToken("videoDetails.author")?.ToString();
        string? primaryArtistId = jsonToken.SelectToken("videoDetails.channelId")?.ToString();

        if (artistNames is null || primaryArtistId is null)
            throw new ArgumentNullException(null, "One or more values of item is null.");

        // Add artists to result
        IEnumerable<string> artists = artistNames.Split(',', '&', '•').Where(artistName => !string.IsNullOrWhiteSpace(artistName)).Select(artistName => artistName.Trim());

        List<ShelfItem> result = [];
        result.Add(new(artists.First(), primaryArtistId, ShelfKind.Artists));
        foreach (string artist in artists.Skip(1))
        {
            result.Add(new(artist, null, ShelfKind.Artists));
        }

        // Return result
        return [.. result];
    }
}