using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents the credits information for a song.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SongCredits"/> class.
/// </remarks>
/// <param name="metadata">The metadata of the song for which the credits are.</param>
/// <param name="performers">The list of performers on the song.</param>
/// <param name="writers">The list of writers of the song.</param>
/// <param name="producers">The list of producers of the song.</param>
/// <param name="metadataProviders">The metadata providers for the song credits.</param>
public class SongCredits(
    SongCredits.Metadata song,
    IReadOnlyList<string> performers,
    IReadOnlyList<string> writers,
    IReadOnlyList<string> producers,
    IReadOnlyList<string> metadataProviders)
{
    /// <summary>
    /// Represents the song for which the credits are.
    /// </summary>
    /// <param name="name">The name of the song.</param>
    /// <param name="id">The ID of the song.</param>
    /// <param name="thumbnails">The thumbnails associated with the song.</param>
    /// <param name="primaryArtist">The primary artist of the song.</param>
    /// <param name="releaseYear">The year the song was released.</param>
    /// <param name="isExplicit">Indicates whether the song is explicit.</param>
    public class Metadata(
        string name,
        string id,
        Thumbnail[] thumbnails,
        YouTubeMusicEntity primaryArtist,
        int? releaseYear,
        bool isExplicit)
    {
        /// <summary>
        /// The name of the song.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// The ID of the song.
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// The thumbnails associated with the song.
        /// </summary>
        public Thumbnail[] Thumbnails { get; } = thumbnails;

        /// <summary>
        /// The primary artist of the song.
        /// </summary>
        public YouTubeMusicEntity PrimaryArtist { get; } = primaryArtist;

        /// <summary>
        /// The year the song was released.
        /// </summary>
        public int? ReleaseYear { get; } = releaseYear;

        /// <summary>
        /// Indicates whether the song is marked as explicit.
        /// </summary>
        public bool IsExplicit { get; } = isExplicit;
    }


    /// <summary>
    /// Parses the JSON element into a <see cref="SongCredits"/>.
    /// </summary>
    /// <param name="element">The JSON element containing "sections" and "metadata".</param>
    internal static SongCredits Parse(
        JsonElement element)
    {
        JsonElement item = element
            .GetProperty("metadata")
            .GetProperty("musicMultiRowListItemRenderer");

        JsonElement titleRun = item
            .GetProperty("title")
            .GetProperty("runs")
            .GetElementAt(0);


        string name = titleRun
            .GetProperty("text")
            .GetString()
            .OrThrow();

        string id = titleRun
            .SelectNavigationVideoId();

        Thumbnail[] thumbnails = item
            .GetProperty("thumbnail")
            .GetProperty("musicThumbnailRenderer")
            .SelectThumbnails();

        YouTubeMusicEntity primaryArtist = item
            .GetProperty("subtitle")
            .GetProperty("runs")
            .GetElementAt(0)
            .SelectArtist();

        int? releaseYear = item
            .GetPropertyOrNull("secondTitle")
            ?.GetPropertyOrNull("runs")
            ?.GetElementAtOrNull(2)
            ?.GetPropertyOrNull("text")
            ?.GetString()
            .ToInt32();

        bool isExplicit = item
            .GetPropertyOrNull("badges")
            .SelectContainsExplicitBadge();

        List<string> performers = [];
        List<string> writers = [];
        List<string> producers = [];
        List<string> metadataProviders = [];
        foreach (JsonElement section in element
            .GetProperty("sections")
            .EnumerateArray())
        {
            if (!section.TryGetProperty("dismissableDialogContentSectionRenderer", out JsonElement sectionRenderer))
                continue;

            string sectionType = sectionRenderer
                .GetProperty("title")
                .GetProperty("runs")
                .GetElementAt(0)
                .GetProperty("text")
                .GetString()
                .OrThrow();

            List<string> targetList = sectionType switch
            {
                "Performed by" => performers,
                "Written by" => writers,
                "Produced by" => producers,
                "Music metadata provided by" => metadataProviders,
                _ => throw new ArgumentException($"Unknown section type: {sectionType}", nameof(element))
            };
            foreach (JsonElement run in sectionRenderer
                .GetProperty("subtitle")
                .GetProperty("runs")
                .EnumerateArray())
            {
                string? text = run
                    .GetProperty("text")
                    .GetString();

                if (!string.IsNullOrEmpty(text) && text != "\n")
                    targetList.Add(text);
            }
        }

        return new(new(name, id, thumbnails, primaryArtist, releaseYear, isExplicit), performers, writers, producers, metadataProviders);
    }


    /// <summary>
    /// The metadata of the song for which the credits are.
    /// </summary>
    public Metadata Song { get; } = song;

    /// <summary>
    /// The list of performers on the song.
    /// </summary>
    public IReadOnlyList<string> Performers { get; } = performers;

    /// <summary>
    /// The list of writers of the song.
    /// </summary>
    public IReadOnlyList<string> Writers { get; } = writers;

    /// <summary>
    /// The list of producers of the song.
    /// </summary>
    public IReadOnlyList<string> Producers { get; } = producers;

    /// <summary>
    /// The metadata providers for the song credits.
    /// </summary>
    public IReadOnlyList<string> MetadataProviders { get; } = metadataProviders;
}