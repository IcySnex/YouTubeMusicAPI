using YouTubeMusicAPI.Json;
using YouTubeMusicAPI.Utils;

namespace YouTubeMusicAPI.Models.Songs;

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
    /// Parses a <see cref="JElement"/> into <see cref="SongCredits"/>.
    /// </summary>
    /// <param name="element">The <see cref="JElement"/> 'dismissableDialogRenderer' to parse.</param>
    /// <returns><see cref="SongCredits"/> representing the <see cref="JElement"/>.</returns>
    internal static SongCredits Parse(
        JElement element)
    {
        // Metadata
        JElement itemRenderer = element
            .Get("metadata")
            .Get("musicMultiRowListItemRenderer");

        SongCreditsMetadata metadata = SongCreditsMetadata.Parse(itemRenderer);

        // Sections
        List<string> performers = [];
        List<string> writers = [];
        List<string> producers = [];
        List<string> metadataProviders = [];

        element
            .Get("sections")
            .AsArray()
            .Or(JArray.Empty)
            .Select(item => item
                .Get("dismissableDialogContentSectionRenderer"))
            .Where(item => !item.IsUndefined)
            .ForEach(item =>
            {
                List<string> enumerable = item
                    .Get("subtitle")
                    .Get("runs")
                    .AsArray()
                    .Or(JArray.Empty)
                    .Select(item => item
                        .Get("text")
                        .AsString())
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Cast<string>()
                    .ToList();

                switch (item
                    .Get("title")
                    .Get("runs")
                    .GetAt(0)
                    .Get("text")
                    .AsString())
                {
                    case "Performed by":
                        performers = enumerable;
                        break;

                    case "Written by":
                        writers = enumerable;
                        break;

                    case "Produced by":
                        producers = enumerable;
                        break;

                    case "Music metadata provided by":
                        metadataProviders = enumerable;
                        break;
                }
            });

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