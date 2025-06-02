using Acornima.Ast;
using System.Text.RegularExpressions;
using YouTubeMusicAPI.Models.Search;

namespace YouTubeMusicAPI.Internal;

/// <summary>
/// Contains extensions methods
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Converts search category to YouTube Music request payload params
    /// </summary>
    /// <param name="value">The YouTube Music item kind to convert</param>
    /// <returns>A YouTube Music request payload params</returns>
    public static string? ToParams(
        this SearchCategory? value) =>
        value switch
        {
            SearchCategory.Songs => "EgWKAQIIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Videos => "EgWKAQIQAWoQEAMQBBAJEAoQBRAREBAQFQ%3D%3D",
            SearchCategory.Albums => "EgWKAQIYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.CommunityPlaylists => "EgeKAQQoAEABahAQAxAKEAkQBBAFEBEQEBAV",
            SearchCategory.Artists => "EgWKAQIgAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Podcasts => "EgWKAQJQAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Episodes => "EgWKAQJIAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            SearchCategory.Profiles => "EgWKAQJYAWoQEAMQChAJEAQQBRAREBAQFQ%3D%3D",
            _ => null
        };

    /// <summary>
    /// Converts a string to YouTube Music search category
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>A SearchCategory</returns>
    public static SearchCategory? ToSearchCategory(
        this string? value) =>
        value switch
        {
            "Songs" => SearchCategory.Songs,
            "Videos" => SearchCategory.Videos,
            "Albums" => SearchCategory.Albums,
            "Community playlists" => SearchCategory.CommunityPlaylists,
            "Artists" => SearchCategory.Artists,
            "Podcasts" => SearchCategory.Podcasts,
            "Episodes" => SearchCategory.Episodes,
            "Profiles" => SearchCategory.Profiles,
            _ => null
        };

    /// <summary>
    /// Converts a type to YouTube Music search category
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>A SearchCategory</returns>
    public static SearchCategory? ToSearchCategory(
        this Type value) =>
        value switch
        {
            _ when value == typeof(SongSearchResult) => SearchCategory.Songs,
            _ when value == typeof(VideoSearchResult) => SearchCategory.Videos,
            _ when value == typeof(CommunityPlaylistSearchResult) => SearchCategory.CommunityPlaylists,
            _ when value == typeof(ArtistSearchResult) => SearchCategory.Artists,
            _ when value == typeof(PodcastSearchResult) => SearchCategory.Podcasts,
            _ when value == typeof(EpisodeSearchResult) => SearchCategory.Episodes,
            _ when value == typeof(ProfileSearchResult) => SearchCategory.Profiles,
            _ => null
        };


    /// <summary>
    /// Parses an exact string into a TimeSpan
    /// </summary>
    /// <param name="value">The string to parse</param>
    /// <returns>A new TimeSpan</returns>
    /// <exception cref="ArgumentException">Occurrs when value has an invalid format</exception>
    public static TimeSpan ToTimeSpan(
        this string value)
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
    public static TimeSpan ToTimeSpanLong(
        this string value)
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
    /// Tries to get the fucntion name and code from a node
    /// </summary>
    /// <param name="value">The node to parse</param>
    /// <param name="fullJs">The full js containing the node</param>
    /// <param name="name">The name of the function</param>
    /// <param name="code">The code of the function</param>
    /// <returns>A boolean indicating weither the function was parsed correctly</returns>
    public static bool TryGetFunctionInfo(
        this Node value,
        string fullJs,
        out string? name,
        out string? code)
    {
        switch (value)
        {
            case FunctionDeclaration funcDecl when funcDecl.Id is not null:
                (name, code) = (funcDecl.Id.Name, fullJs.Substring(funcDecl.Start, funcDecl.End - funcDecl.Start));
                return true;

            case VariableDeclaration varDecl:
                foreach (VariableDeclarator decl in varDecl.Declarations)
                {
                    if (decl.Id is not Identifier id || decl.Init is not FunctionExpression)
                        continue;

                    (name, code) = (id.Name, fullJs.Substring(decl.Start, decl.End - decl.Start));
                    return true;
                }
                break;

            case ExpressionStatement { Expression: AssignmentExpression { Left: Identifier id, Right: FunctionExpression } }:
                (name, code) = (id.Name, fullJs.Substring(value.Start, value.End - value.Start));
                return true;
        }

        (name, code) = (null, null);
        return false;
    }
}