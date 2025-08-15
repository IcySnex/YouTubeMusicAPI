using System.Text.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models;

/// <summary>
/// Represents the credits information for a song.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="SongCredits"/> class.
/// </remarks>
/// <param name="metadata">The song metadata of the song for which the credits are.</param>
/// <param name="performers">The list of performers on the song.</param>
/// <param name="writers">The list of writers of the song.</param>
/// <param name="producers">The list of producers of the song.</param>
/// <param name="metadataProviders">The metadata providers for the song credits.</param>
public class SongCredits(
    SongCreditsMetadata metadata,
    IReadOnlyList<string> performers,
    IReadOnlyList<string> writers,
    IReadOnlyList<string> producers,
    IReadOnlyList<string> metadataProviders)
{
    /// <summary>
    /// Parses a <see cref="JsonElement"/> into <see cref="SongCredits"/>.
    /// </summary>
    /// <param name="element">The <see cref="JsonElement"/> 'dismissableDialogRenderer' to parse.</param>
    internal static SongCredits Parse(
        JsonElement element)
    {
        // Metadata
        JsonElement itemRenderer = element
            .GetProperty("metadata")
            .GetProperty("musicMultiRowListItemRenderer");

        SongCreditsMetadata metadata = SongCreditsMetadata.Parse(itemRenderer);

        // Sections
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
                .GetPropertyAt(0)
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

        return new(metadata, performers, writers, producers, metadataProviders);
    }


    /// <summary>
    /// The song metadata of the song for which the credits are.
    /// </summary>
    public SongCreditsMetadata Metadata { get; } = metadata;

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